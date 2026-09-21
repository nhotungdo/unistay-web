namespace UnistayWeb.BLL.ViewModels
{
    public class ConnectionRequestDto
    {
        public string? TargetUserId { get; set; }
    }

    public class RespondRequestDto
    {
        public int ConnectionId { get; set; }
        public string? Action { get; set; }
    }

    public class PagedUserSearchResultDto
    {
        public List<UserSearchResultDto> Data { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
    }

    public class UserSearchResultDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        public string ConnectionStatus { get; set; } = "none";
    }

    public class PendingRequestDto
    {
        public int ConnectionId { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterEmail { get; set; }
        public string? RequesterAvatar { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class FriendDto
    {
        public string FriendId { get; set; } = string.Empty;
        public string FriendName { get; set; } = "Người dùng";
        public string FriendAvatar { get; set; } = "/images/default-avatar.png";
        public DateTime? ConnectedSince { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConnectionSuggestionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? MutualInfo { get; set; }
        public int Score { get; set; }
    }
}
