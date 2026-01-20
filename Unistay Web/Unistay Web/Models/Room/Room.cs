using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Room
{
    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        public string OwnerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        public decimal? Deposit { get; set; }
        
        public decimal Area { get; set; }
        
        public int MaxOccupants { get; set; }
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public string? Amenities { get; set; }
        
        public string? Rules { get; set; }
        
        public string Status { get; set; } = "Available";
        
        public bool IsFeatured { get; set; }
        
        public bool IsVIP { get; set; }
        
        public int ViewCount { get; set; }
        
        public int ContactCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
}
