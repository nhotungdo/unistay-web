using Microsoft.AspNetCore.Mvc;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok(new { message = "Admin Dashboard data" });
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            return Ok(new { data = new List<object>() });
        }

        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            return Ok(new { data = new List<object>() });
        }

        [HttpGet("reports")]
        public IActionResult GetReports()
        {
            return Ok(new { data = new List<object>() });
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            return Ok(new { data = new { totalUsers = 0, totalRooms = 0 } });
        }
    }
}
