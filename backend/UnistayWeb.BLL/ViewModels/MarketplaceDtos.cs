using Microsoft.AspNetCore.Http;

namespace UnistayWeb.BLL.ViewModels
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
}
