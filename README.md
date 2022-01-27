# BlendoBot.Module.Weather
## Returns the weather for a given address
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/BlendoBot/BlendoBot.Module.Weather/Tests)

Can't figure if it's sunny or raining? This command lets you get that information directly through a Discord bot!

## Discord Usage
- `?weather [location]`
  - Shows the weather for a given location.

## Config
This module requires two API keys: a `TimezoneApiKey` from [TimeZoneDB](https://timezonedb.com/) and a `WeatherApiKey` from [OpenWeatherMap](https://openweathermap.org/api).
```cfg
[Weather]
TimezoneApiKey=YOUR_TIMEZONEDB_API_KEY
WeatherApiKey=YOUR_OPENWEATHERMAP_API_KEY
```