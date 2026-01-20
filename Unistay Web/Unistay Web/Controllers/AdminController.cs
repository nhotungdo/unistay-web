using Microsoft.AspNetCore.Mvc;

namespace Unistay_Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Rooms()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            return View();
        }
    }
}
