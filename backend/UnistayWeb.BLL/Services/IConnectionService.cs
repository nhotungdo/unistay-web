using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.Connection;

namespace UnistayWeb.BLL.Services
{
    public interface IConnectionService
    {
        Task<PagedUserSearchResultDto> SearchUsersAsync(string? query, int page, string currentUserId);
        Task<ServiceResult> SendRequestAsync(string targetUserId, string currentUserId);
        Task<ServiceResult> RespondRequestAsync(int connectionId, string action, string currentUserId);
        Task<List<PendingRequestDto>> GetPendingRequestsAsync(string currentUserId);
        Task<List<FriendDto>> GetFriendsAsync(string currentUserId);
        Task<List<ConnectionSuggestionDto>> GetSuggestionsAsync(string currentUserId);
        Task<List<string>> GetUserFriendIdsAsync(string userId);
        Task<List<ChatGroup>> GetUserGroupsAsync(string userId);
        Task<List<ConnectionUserDto>> GetUserFriendsAsync(string userId);
    }

    public class ConnectionUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
