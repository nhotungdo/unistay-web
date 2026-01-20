using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Models.Moving;

namespace Unistay_Web.Controllers
{
    public class MovingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(MovingRequest request)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
