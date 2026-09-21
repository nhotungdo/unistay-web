using Microsoft.AspNetCore.Mvc;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { message = "Unistay Web API is running." });
        }
    }
}
