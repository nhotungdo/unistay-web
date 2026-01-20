using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Marketplace
{
    public class MarketplaceItem
    {
        public int Id { get; set; }
        
        [Required]
        public string SellerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        public string? Condition { get; set; }
        
        public string? Location { get; set; }
        
        public string? ImageUrls { get; set; }
        
        public string Status { get; set; } = "Available";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
