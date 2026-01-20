using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Review
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
        
        public string? ImageUrls { get; set; }
        
        public bool IsVerifiedTenant { get; set; }
        
        public string? OwnerResponse { get; set; }
        
        public bool IsVisible { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
