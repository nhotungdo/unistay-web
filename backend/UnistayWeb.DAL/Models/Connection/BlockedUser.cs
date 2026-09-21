using System;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.DAL.Models.Connection
{
    public class BlockedUser
    {
        public int Id { get; set; }
        
        // Người chặn
        public string? BlockerId { get; set; }
        public UserProfile? Blocker { get; set; }
        
        // Người bị chặn
        public string? BlockedUserId { get; set; }
        public UserProfile? BlockedUserProfile { get; set; }
        
        public string? Reason { get; set; }
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
}
