using UnistayWeb.BLL.ViewModels;

namespace UnistayWeb.BLL.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int HttpStatusCode { get; set; } = 200;
    }

    public interface IAccountService
    {
        Task<LoginResultDto> LoginAsync(string email, string password);
        Task<AuthResponseDto> RegisterAsync(string email, string fullName, string phone, string password);
        Task LogoutAsync(string userId, string? email);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task LogLoginHistoryAsync(string? userId, string? email, bool isSuccessful, string? failureReason, string? authMethod = "Password", DateTime? logoutTime = null);
        Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress = null);
    }

    public class LoginResultDto
    {
        public bool IsSuccess { get; set; }
        public AuthResponseDto? Response { get; set; }
        public string? ErrorMessage { get; set; }
        public int HttpStatusCode { get; set; } = 200;
    }
}
