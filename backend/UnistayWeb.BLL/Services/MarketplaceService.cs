using Microsoft.EntityFrameworkCore;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Marketplace;

namespace UnistayWeb.BLL.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MarketplaceService> _logger;

        public MarketplaceService(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<MarketplaceService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<MarketplaceItem>> GetItemsAsync()
        {
            return await _context.MarketplaceItems.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task<MarketplaceItem?> GetItemAsync(int id)
        {
            return await _context.MarketplaceItems.FindAsync(id);
        }

        public async Task<MarketplaceItem> CreateItemAsync(CreateMarketplaceItemDto dto, string userId, string webRootPath)
        {
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

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(webRootPath, "images", "marketplace");
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

            return item;
        }
    }
}
