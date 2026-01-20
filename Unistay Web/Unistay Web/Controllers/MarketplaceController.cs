using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Models.Marketplace;

namespace Unistay_Web.Controllers
{
    public class MarketplaceController : Controller
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
        public IActionResult Create(MarketplaceItem item)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }
    }
}
