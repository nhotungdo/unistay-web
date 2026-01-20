using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Service
{
    public class RoomFinderRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public decimal Budget { get; set; }
        
        public string? PreferredArea { get; set; }
        
        public string? Requirements { get; set; }
        
        public DateTime NeededBy { get; set; }
        
        public decimal ServiceFee { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public string? AssignedTo { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
    }
}
