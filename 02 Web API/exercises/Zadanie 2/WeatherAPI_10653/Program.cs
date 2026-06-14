var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var windDirections = new[]
{
    "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West"
};

app.MapGet("/temperature", () =>
{
    int temperatureC = Random.Shared.Next(-30, 46);

    return new
    {
        TemperatureC = temperatureC,
        TemperatureF = 32 + (int)(temperatureC / 0.5556)
    };
});

app.MapGet("/wind", () =>
{
    string windDirection = windDirections[Random.Shared.Next(windDirections.Length)];

    return new
    {
        WindDirection = windDirection
    };
});

app.MapGet("/info", () =>
{
    return "Witaj w mojej aplikacji Weather API!\nAplikacja przedstawia temperaturę w Fahrenheitach oraz Celsjuszach w najbliższych 5 kolejnych dniach (od jutra) wraz z krótkim podsumowaniem.\n\nZadanie wykonane przez:\nNatalia Górecka\nNr albumu: 10653";
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-30, 46),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
