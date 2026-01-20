using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Models.Room;

namespace Unistay_Web.Controllers
{
    public class RoomsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Room room)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, Room room)
        {
            if (ModelState.IsValid)
            {
                // TODO: Update database
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(room);
        }

        public IActionResult Search()
        {
            return View();
        }
    }
}
