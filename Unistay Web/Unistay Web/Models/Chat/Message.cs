using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Chat
{
    public class Message
    {
        public int Id { get; set; }
        
        [Required]
        public string SenderId { get; set; } = string.Empty;
        
        [Required]
        public string ReceiverId { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? AttachmentUrl { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
