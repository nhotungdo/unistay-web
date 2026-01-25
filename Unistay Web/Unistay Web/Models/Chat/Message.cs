using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Chat
{
    public class Message
    {
        public int Id { get; set; }
        
        [Required]
        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual UserProfile Sender { get; set; }
        
        public string? ReceiverId { get; set; }
        
        [Required]
        public int ConversationId { get; set; }
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? AttachmentUrl { get; set; }

        public string Type { get; set; } = "Text"; // Text, Image, File
        
        public bool IsRead { get; set; } = false;
        
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
