using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Notification
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Message { get; set; }
        
        public string Type { get; set; } = "Info";
        
        public bool IsRead { get; set; }
        
        public string? ActionUrl { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
