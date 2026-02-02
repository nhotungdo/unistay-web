using System;

namespace Unistay_Web.Models.Connection
{
    public class SendMessageDto
    {
        public string? ReceiverId { get; set; }
        public int? GroupId { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public bool IsEncrypted { get; set; } = false;
        public int? ReplyToMessageId { get; set; }
        public List<string>? AttachmentPaths { get; set; }
    }

    public class BlockUserDto
    {
        public string? UserId { get; set; }
        public string? Reason { get; set; }
    }

    public class ConversationViewModel
    {
        public string? UserId { get; set; }
        public int? GroupId { get; set; }
        public bool IsGroup { get; set; }
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }
        public MessageViewModel? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }

    public class MessageViewModel
    {
        public string? Content { get; set; }
        public string? Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSent { get; set; }
    }
    

    public class ReportMessageDto
    {
        public int MessageId { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
    }
}
