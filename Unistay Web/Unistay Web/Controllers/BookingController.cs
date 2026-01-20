using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Models.Booking;

namespace Unistay_Web.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create(int roomId)
        {
            ViewBag.RoomId = roomId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save appointment
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }

        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
