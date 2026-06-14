using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

app.UseHttpsRedirection();

const int MinTemperature = -30;
const int MaxTemperature = 46;
const string ApiKey = "6e35f7bbbd4166ee42fbdd5c798190d8";

app.MapGet("/", () =>
{
    return Results.Redirect("/weather/warsaw");
});

app.MapGet("/info", () =>
{
    return "Zadanie wykonane przez:\n" +
           "Natalia Górecka\n" +
           "Nr albumu: 10653";
});

app.MapGet("/weather/{city}", async (string city, IHttpClientFactory httpClientFactory) =>
{
    HttpClient httpClient = httpClientFactory.CreateClient();

    string cityEncoded = Uri.EscapeDataString(city);
    string url = $"https://api.openweathermap.org/data/2.5/forecast?q={cityEncoded}&appid={ApiKey}&units=metric&lang=pl";

    HttpResponseMessage response = await httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest(new
        {
            Message = "Nie udało się pobrać prognozy pogody.",
            City = city,
            StatusCode = response.StatusCode.ToString()
        });
    }

    WeatherForecastResponse? weatherData =
        await response.Content.ReadFromJsonAsync<WeatherForecastResponse>();

    var dailyForecast = weatherData.Forecasts
        .Where(item => item.Main != null &&
                       item.Wind != null &&
                       !string.IsNullOrEmpty(item.DateText))
        .GroupBy(item => DateTime.Parse(item.DateText!).Date)
        .Where(group => group.Key > DateTime.Today)
        .Take(5)
        .Select(group => new
        {
            Date = DateOnly.FromDateTime(group.Key),
            MinTemperatureC = (int)Math.Round(group.Min(item => item.Main!.Temperature), MidpointRounding.AwayFromZero),
            MaxTemperatureC = (int)Math.Round(group.Max(item => item.Main!.Temperature), MidpointRounding.AwayFromZero),
            Weather = group.First().Weather.FirstOrDefault()?.Description,
            AverageWindSpeed = Math.Round(group.Average(item => item.Wind!.Speed), 1)
        });

    return Results.Ok(new
    {
        City = weatherData.City?.Name,
        Country = weatherData.City?.Country,
        Forecast = dailyForecast
    });
});

app.Run();

public class WeatherForecastResponse
{
    [JsonPropertyName("city")]
    public WeatherCity? City { get; set; }

    [JsonPropertyName("list")]
    public List<WeatherForecastItem> Forecasts { get; set; } = new();
}

public class WeatherCity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

public class WeatherForecastItem
{
    [JsonPropertyName("dt_txt")]
    public string? DateText { get; set; }

    [JsonPropertyName("main")]
    public WeatherMain? Main { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();

    [JsonPropertyName("wind")]
    public WeatherWind? Wind { get; set; }
}

public class WeatherMain
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class WeatherWind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}