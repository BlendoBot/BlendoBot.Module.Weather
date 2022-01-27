using BlendoBot.Core.Command;
using BlendoBot.Core.Entities;
using BlendoBot.Core.Module;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlendoBot.Module.Weather;

internal class WeatherCommand : ICommand {
	public WeatherCommand(Weather module) {
		this.module = module;
	}

	private readonly Weather module;
	public IModule Module => module;

	public string Guid => "weather.command";
	public string DesiredTerm => "weather";
	public string Description => "Returns the weather for a given address";
	public Dictionary<string, string> Usage => new() {
		{ "[location]", "Shows the weather for the given location" }
	};
		
	public async Task OnMessage(MessageCreateEventArgs e, string[] tokenizedInput) {
		if (e.Message.Content.Length < 9) {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"Too few arguments specified to {module.ModuleManager.GetCommandTermWithPrefix(this)}",
				Channel = e.Channel,
				Tag = "WeatherErrorTooFewArgs"
			});
			return;
		}

		string locationInput = string.Join(' ', tokenizedInput);

		WeatherResult weatherResult = await module.GetWeather(locationInput);

		if (weatherResult.ResultCode == 200) {
			StringBuilder sb = new();
			sb.AppendLine($"Weather for **{weatherResult.Location}**, {weatherResult.Country} *({weatherResult.Latitude}, {weatherResult.Longitude})*");
			sb.AppendLine($"Temperature: {weatherResult.TemperatureCurrent.Celsius}°C (low: {weatherResult.TemperatureMin.Celsius}°C, high: {weatherResult.TemperatureMax.Celsius}°C)");
			sb.AppendLine($"Current condition: {weatherResult.Condition}");
			sb.AppendLine($"Pressure: {weatherResult.PressureHPA}hPa");
			sb.AppendLine($"Wind: {weatherResult.WindSpeed}kmh at {weatherResult.WindDirection}°T");
			sb.AppendLine($"Sunrise: {weatherResult.Sunrise:hh:mm:ss tt}, Sunset: {weatherResult.Sunset:hh:mm:ss tt} *({weatherResult.TimeZone})*");

			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = sb.ToString(),
				Channel = e.Channel,
				Tag = "WeatherSuccess"
			});
		} else {
			await module.DiscordInteractor.Send(this, new SendEventArgs {
				Message = $"API returned a bad error ({weatherResult.ResultCode}): *{weatherResult.ResultMessage}*",
				Channel = e.Channel,
				Tag = "WeatherErrorAPINotOK"
			});
		}
	}
}
