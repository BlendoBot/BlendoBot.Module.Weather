namespace BlendoBot.Module.Weather;

internal struct Temperature {
	public decimal Kelvin {
		get => Celsius + 273.15m;
		set => Celsius = value - 273.15m;
	}

	public decimal Celsius { get; set; }

	public decimal Farenheit {
		get => Celsius * 9.0m / 5.0m + 32.0m;
		set => Celsius = (value - 32.0m) * 5.0m / 9.0m;
	}

	public static implicit operator decimal(Temperature d) => d.Celsius;

	public static implicit operator Temperature(decimal d) => FromCelsius(d);

	public static Temperature FromCelsius(decimal d) => new() { Celsius = d };

	public static Temperature FromFarenheit(decimal d) => new() { Farenheit = d };

	public static Temperature FromKelvin(decimal d) => new() { Kelvin = d };
}
