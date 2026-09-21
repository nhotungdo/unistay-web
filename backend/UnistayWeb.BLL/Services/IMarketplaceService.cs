using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.Marketplace;

namespace UnistayWeb.BLL.Services
{
    public interface IMarketplaceService
    {
        Task<List<MarketplaceItem>> GetItemsAsync();
        Task<MarketplaceItem?> GetItemAsync(int id);
        Task<MarketplaceItem> CreateItemAsync(CreateMarketplaceItemDto dto, string userId, string webRootPath);
    }
}
