using Microsoft.AspNetCore.Mvc;
using UnistayWeb.DAL.Models.Booking;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetBookings()
        {
            // TODO: Return bookings
            return Ok(new { data = new List<object>() });
        }

        [HttpPost]
        public IActionResult Create([FromBody] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save appointment
                return Ok(new { message = "Đặt lịch thành công" });
            }
            return BadRequest(ModelState);
        }

        [HttpGet("{id}")]
        public IActionResult GetBooking(int id)
        {
            // TODO: Return booking details
            return Ok(new { id = id });
        }
    }
}
