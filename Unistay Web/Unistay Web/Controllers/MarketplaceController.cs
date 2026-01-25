using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Unistay_Web.Data;
using Unistay_Web.Models.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Unistay_Web.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MarketplaceController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.MarketplaceItems.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.MarketplaceItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarketplaceItem item, IFormFile? imageFile)
        {
            // SellerId will be set from the logged-in user, so remove it from validation
            ModelState.Remove("SellerId");

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    // Should not happen due to [Authorize]
                    return Unauthorized();
                }

                item.SellerId = userId;
                item.CreatedAt = DateTime.Now;
                item.Status = "Available"; // Default status

                // Handle Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 1. Create directory if not exists
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "marketplace");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // 2. Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // 3. Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // 4. Set relative path in model
                    item.ImageUrls = "/images/marketplace/" + uniqueFileName;
                }

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }
    }
}
