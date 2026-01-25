using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Chat
{
    public class Conversation
    {
        public int Id { get; set; }
        
        public string? Name { get; set; } // For Group chats, or optional for 1-on-1

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserConversation> UserConversations { get; set; } = new List<UserConversation>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
