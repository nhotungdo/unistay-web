using System;

namespace Unistay_Web.Models.Connection
{
    public class MessageAttachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Message? Message { get; set; }
        
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; } // image/png, application/pdf, etc.
        public long FileSize { get; set; } // in bytes
        public string? ThumbnailPath { get; set; } // for images/videos
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
