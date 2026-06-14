namespace WeatherAPI_10653.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http.Json;

    [ApiController]
    [Route("weather")]
    public class WeatherController : ControllerBase
    {
        private const string ApiKey = "6e35f7bbbd4166ee42fbdd5c798190d8";
        private readonly IHttpClientFactory _httpClientFactory;
        public WeatherController(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeatherForecastForCity(string city)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            string cityEncoded = Uri.EscapeDataString(city);
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={cityEncoded}&appid={ApiKey}&units=metric&lang=pl";

            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    Message = "Nie udało się pobrać prognozy pogody.",
                    City = city,
                    StatusCode = response.StatusCode.ToString()
                });
            }

            WeatherForecastResponse? weatherData =
                await response.Content.ReadFromJsonAsync<WeatherForecastResponse>();

            if (weatherData == null)
            {
                return BadRequest(new
                {
                    Message = "Nie udało się odczytać danych pogodowych."
                });
            }

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

            return Ok(new
            {
                City = weatherData.City?.Name,
                Country = weatherData.City?.Country,
                Forecast = dailyForecast
            });
        }

        private static readonly List<City> Cities = new();
        private static int nextCityId = 1;

        [HttpPost("cities")]
        public IActionResult AddCity([FromBody] CityRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { Message = "Nazwa miasta nie może być pusta." });
            }

            string cityName = request.Name.Trim();

            if (Cities.Any(city => city.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new
                {
                    Message = "To miasto już znajduje się w kolekcji.",
                    City = cityName
                });
            }

            var city = new City
            {
                Id = nextCityId++,
                Name = cityName
            };

            Cities.Add(city);

            return Ok(new
            {
                Message = "Miasto zostało dodane do kolekcji.",
                City = city
            });
        }

        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            return Ok(Cities);
        }

        [HttpGet("cities/{id:int}")]
        public IActionResult GetCityById(int id)
        {
            var city = Cities.FirstOrDefault(city => city.Id == id);

            if (city == null)
            {
                return NotFound(new
                {
                    Message = "Nie znaleziono miasta o podanym id.",
                    Id = id
                });
            }

            return Ok(city);
        }

        [HttpDelete("cities/{id:int}")]
        public IActionResult DeleteCity(int id)
        {
            var city = Cities.FirstOrDefault(city => city.Id == id);

            if (city == null)
            {
                return NotFound(new
                {
                    Message = "Nie znaleziono miasta o podanym id.",
                    Id = id
                });
            }

            Cities.Remove(city);

            return Ok(new
            {
                Message = "Miasto zostało usunięte.",
                City = city
            });
        }

        [HttpPut("cities/{id:int}")]
        public IActionResult UpdateCity(int id, [FromBody] CityRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new
                {
                    Message = "Nowa nazwa miasta nie może być pusta."
                });
            }

            var city = Cities.FirstOrDefault(city => city.Id == id);

            if (city == null)
            {
                return NotFound(new
                {
                    Message = "Nie znaleziono miasta o podanym id.",
                    Id = id
                });
            }

            string newCityName = request.Name.Trim();

            if (Cities.Any(existingCity =>
                    existingCity.Id != id &&
                    existingCity.Name.Equals(newCityName, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new
                {
                    Message = "Miasto o takiej nazwie już istnieje w kolekcji.",
                    City = newCityName
                });
            }

            city.Name = newCityName;

            return Ok(new
            {
                Message = "Miasto zostało zaktualizowane.",
                City = city
            });
        }
    }

    public class CityRequest
    {
        public string? Name { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
