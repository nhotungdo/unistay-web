using System;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Connection
{
    public enum MessageType { Text = 0, Image = 1, File = 2, Emoji = 3 }
    public enum MessageStatus { Sent = 0, Delivered = 1, Seen = 2 }

    public class Message
    {
        public int Id { get; set; }
        
        // Sender & Receiver
        public string? SenderId { get; set; }
        public UserProfile? Sender { get; set; }
        
        public string? ReceiverId { get; set; }
        public UserProfile? Receiver { get; set; }
        
        // Group chat support
        public int? ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }
        
        // Message content
        public string? Content { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        
        // Security
        public bool IsEncrypted { get; set; } = false;
        
        // Edit & Delete
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        
        // Reply feature
        public int? ReplyToMessageId { get; set; }
        public Message? ReplyToMessage { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? SeenAt { get; set; }
        
        // Attachments
        public ICollection<MessageAttachment>? Attachments { get; set; }
    }
}
