using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Chat
{
    public class UserConversation
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }

        public int ConversationId { get; set; }
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
