namespace UnistayWeb.BLL.ViewModels
{
    public class MessageResultDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }
        public bool IsSent { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? SeenAt { get; set; }
        public ReplyMessageDto? ReplyTo { get; set; }
        public List<MessageAttachmentDto>? Attachments { get; set; }
    }

    public class ReplyMessageDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? SenderId { get; set; }
    }

    public class MessageAttachmentDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public string? ThumbnailPath { get; set; }
    }

    public class SendMessageResultDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public int? GroupId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string>? AttachmentPaths { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class FileUploadResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
