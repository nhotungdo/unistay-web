using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Roommate
{
    public class RoommateProfile
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string? Gender { get; set; }
        
        public decimal Budget { get; set; }
        
        public string? PreferredArea { get; set; }
        
        public DateTime? MoveInDate { get; set; }
        
        public string? Habits { get; set; }
        
        public string Status { get; set; } = "Looking";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
