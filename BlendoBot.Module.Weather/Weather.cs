using BlendoBot.Core.Entities;
using BlendoBot.Core.Module;
using BlendoBot.Core.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlendoBot.Module.Weather;

[Module(Guid = "com.biendeo.blendobot.module.weather", Name = "Weather", Author = "Biendeo", Version = "2.0.0", Url = "https://github.com/BlendoBot/BlendoBot.Module.Weather")]
public class Weather : IModule {
	public Weather(IConfig config, IDiscordInteractor discordInteractor, IModuleManager moduleManager, ILogger logger) {
		Config = config;
		DiscordInteractor = discordInteractor;
		ModuleManager = moduleManager;
		Logger = logger;

		WeatherCommand = new(this);
	}

	internal ulong GuildId { get; private set; }

	internal readonly WeatherCommand WeatherCommand;

	internal readonly IConfig Config;
	internal readonly IDiscordInteractor DiscordInteractor;
	internal readonly IModuleManager ModuleManager;
	internal readonly ILogger Logger;

	internal string WeatherApiKey { get; private set; }
	internal string TimezoneApiKey { get; private set; }

	public Task<bool> Startup(ulong guildId) {
		GuildId = guildId;
		WeatherApiKey = Config.ReadConfig(this, "Weather", "WeatherApiKey");
		if (WeatherApiKey == null) {
			Config.WriteConfig(this, "Weather", "WeatherApiKey", "PLEASE ADD API KEY");
			Logger.Log(this, new LogEventArgs {
				Type = LogType.Error,
				Message = $"BlendoBot Weather has not been supplied a valid weather API key!  Please acquire a weather API key from https://openweathermap.org/api, and add it to the config under the [Weather] section."
			});
		}
		TimezoneApiKey = Config.ReadConfig(this, "Weather", "TimezoneApiKey");
		if (TimezoneApiKey == null) {
			Config.WriteConfig(this, "Weather", "TimezoneApiKey", "PLEASE ADD API KEY");
			Logger.Log(this, new LogEventArgs {
				Type = LogType.Error,
				Message = $"BlendoBot Weather has not been supplied a valid timezone API key!  Please acquire a timezone API key from https://timezonedb.com/, and add it to the config under the [Weather] section."
			});
		}
		return Task.FromResult(WeatherApiKey != null && TimezoneApiKey != null && ModuleManager.RegisterCommand(this, WeatherCommand, out _));
	}

	internal async Task<WeatherResult> GetWeather(string inputLocation) {
		string weatherJsonString = "";
		try {
			using HttpClient wc = new();
			weatherJsonString = await wc.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?q={inputLocation.Replace(" ", "+")}&type=like&mode=json&APPID={WeatherApiKey}");
		} catch (WebException) {
			return new WeatherResult {
				ResultCode = 404,
				ResultMessage = $"city not found"
			};
		}
		dynamic weatherJson = JsonConvert.DeserializeObject(weatherJsonString);
		if (weatherJson.cod == 200) {
			string timezoneJsonString = "";
			using (HttpClient wc = new()) {
				timezoneJsonString = await wc.GetStringAsync($"http://api.timezonedb.com/v2.1/get-time-zone?key={TimezoneApiKey}&format=json&by=position&lat={weatherJson.coord.lat}&lng={weatherJson.coord.lon}");
			}
			//TODO: Probably check that this call worked.
			dynamic timezoneJson = JsonConvert.DeserializeObject(timezoneJsonString);
			return new WeatherResult {
				ResultCode = weatherJson.cod,
				ResultMessage = weatherJson.message,
				Condition = weatherJson.weather[0].main,
				TemperatureCurrent = Temperature.FromKelvin((decimal)weatherJson.main.temp),
				PressureHPA = weatherJson.main.pressure,
				TemperatureMin = Temperature.FromKelvin((decimal)weatherJson.main.temp_min),
				TemperatureMax = Temperature.FromKelvin((decimal)weatherJson.main.temp_max),
				WindSpeed = weatherJson.wind.speed,
				WindDirection = weatherJson.wind.deg,
				Sunrise = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)weatherJson.sys.sunrise).AddSeconds((double)timezoneJson.gmtOffset),
				Sunset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)weatherJson.sys.sunset).AddSeconds((double)timezoneJson.gmtOffset),
				Location = weatherJson.name,
				Country = weatherJson.sys.country,
				Latitude = (decimal)weatherJson.coord.lat,
				Longitude = (decimal)weatherJson.coord.lon,
				TimeZone = $"UTC{(timezoneJson.gmtOffset >= 0 ? "+" : "")}{(int)timezoneJson.gmtOffset / 3600:00}:{(int)timezoneJson.gmtOffset / 60 % 60:00}"
			};
		} else {
			return new WeatherResult {
				ResultCode = weatherJson.cod,
				ResultMessage = weatherJson.message
			};
		}
	}
}
