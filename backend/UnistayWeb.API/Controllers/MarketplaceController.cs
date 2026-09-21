using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace UnistayWeb.API.Controllers
{
    public class CreateMarketplaceItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MarketplaceController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Marketplace
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.MarketplaceItems.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return Ok(items);
        }

        // GET: api/Marketplace/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.MarketplaceItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }
            return Ok(item);
        }

        // POST: api/Marketplace
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> CreateItem([FromForm] CreateMarketplaceItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var item = new MarketplaceItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Condition = dto.Condition,
                Location = dto.Location,
                Category = dto.Category,
                SellerId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Available"
            };

            // Handle Image Upload
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "marketplace");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                item.ImageUrls = "/images/marketplace/" + uniqueFileName;
            }

            _context.MarketplaceItems.Add(item);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }
    }
}
