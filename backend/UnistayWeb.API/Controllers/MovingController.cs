using Microsoft.AspNetCore.Mvc;
using UnistayWeb.DAL.Models.Moving;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovingController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetRequests()
        {
            return Ok(new { data = new List<object>() });
        }

        [HttpPost]
        public IActionResult Create([FromBody] MovingRequest request)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                return Ok(new { message = "Yêu cầu chuyển trọ đã được tạo" });
            }
            return BadRequest(ModelState);
        }

        [HttpGet("{id}")]
        public IActionResult GetRequest(int id)
        {
            return Ok(new { id = id });
        }
    }
}
