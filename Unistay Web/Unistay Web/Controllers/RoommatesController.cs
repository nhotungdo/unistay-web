using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Models.Roommate;

namespace Unistay_Web.Controllers
{
    public class RoommatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile(int id)
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RoommateProfile profile)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        public IActionResult Match()
        {
            return View();
        }
    }
}
