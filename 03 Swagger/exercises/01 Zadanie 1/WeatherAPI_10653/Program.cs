using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/info", () =>
{
    return "Zadanie wykonane przez:\n" +
           "Natalia Górecka\n" +
           "Nr albumu: 10653";
});

app.MapControllers();
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