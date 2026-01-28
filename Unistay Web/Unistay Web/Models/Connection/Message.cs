using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Connection
{
    public enum MessageStatus
    {
        Sent,
        Delivered,
        Seen
    }

    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual UserProfile Sender { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual UserProfile Receiver { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }
}
