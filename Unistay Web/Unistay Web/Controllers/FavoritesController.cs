using Microsoft.AspNetCore.Mvc;

namespace Unistay_Web.Controllers
{
    public class FavoritesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(int roomId)
        {
            // TODO: Add to favorites
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Remove(int roomId)
        {
            // TODO: Remove from favorites
            return Json(new { success = true });
        }
    }
}
