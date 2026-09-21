using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.Connection;

namespace UnistayWeb.BLL.Services
{
    public interface IMessageService
    {
        Task<List<ConversationViewModel>> GetConversationsAsync(string currentUserId);
        Task<PagedResultDto<MessageResultDto>> GetMessagesAsync(string currentUserId, string userId, int page, int pageSize);
        Task<PagedResultDto<MessageResultDto>> GetGroupMessagesAsync(string currentUserId, int groupId, int page, int pageSize);
        Task<SendMessageResultDto?> CreateMessageAsync(string currentUserId, SendMessageDto model, List<string>? attachmentPaths = null);
        Task<bool> MarkAsSeenAsync(string currentUserId, int messageId);
        Task<bool> DeleteMessageAsync(string currentUserId, int messageId);
        Task<bool> IsUserBlockedAsync(string userId1, string userId2);
        Task BlockUserAsync(string currentUserId, string blockedUserId, string? reason);
        Task<bool> UnblockUserAsync(string currentUserId, string blockedUserId);
        Task ReportMessageAsync(string currentUserId, int messageId, ReportReason reason, string? description);
        Task<List<MessageSearchResultDto>> SearchMessagesAsync(string currentUserId, string query, string? userId);
        Task<FileUploadResultDto> UploadMessageFileAsync(string currentUserId, IFormFile file, string webRootPath);
        Task MarkConversationAsSeenAsync(string currentUserId, string senderId);
        Task UpdateMessageStatusToDeliveredAsync(string currentUserId, int messageId);
        Task<int> CreateGroupAsync(string creatorId, string name, List<string> members);
    }

    public class MessageSearchResultDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
