using System;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Connection
{
    public enum ReportReason
    {
        Spam = 0,
        Harassment = 1,
        InappropriateContent = 2,
        FakeProfile = 3,
        Other = 99
    }

    public enum ReportStatus
    {
        Pending = 0,
        UnderReview = 1,
        Resolved = 2,
        Dismissed = 3
    }

    public class MessageReport
    {
        public int Id { get; set; }
        
        // Người báo cáo
        public string? ReporterId { get; set; }
        public UserProfile? Reporter { get; set; }
        
        // Tin nhắn bị báo cáo
        public int MessageId { get; set; }
        public Message? Message { get; set; }
        
        // Thông tin báo cáo
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNote { get; set; }
    }
}
