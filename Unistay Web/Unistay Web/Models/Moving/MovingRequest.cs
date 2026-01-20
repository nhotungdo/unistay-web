using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Moving
{
    public class MovingRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string FromAddress { get; set; } = string.Empty;
        
        [Required]
        public string ToAddress { get; set; } = string.Empty;
        
        public DateTime PreferredDate { get; set; }
        
        public string? ItemsList { get; set; }
        
        public string? Notes { get; set; }
        
        public decimal? EstimatedCost { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
