using Microsoft.AspNetCore.Mvc;

namespace WeatherAPI_10653.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestErrorsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            throw new Exception("Testowy wyjątek");
        }
    }
}
