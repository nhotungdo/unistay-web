using Microsoft.AspNetCore.Identity;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileAsync(string userId);
        Task<IdentityResult> EditProfileAsync(string userId, EditProfileDto dto);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<PagedHistoryDto<LoginHistory>> GetLoginHistoryAsync(string userId, int page);
        Task<PagedHistoryDto<ActivityHistory>> GetActivityHistoryAsync(string userId, int page);
        Task<bool> VerifyEmailAsync(string userId);
        Task<PublicProfileDto> GetPublicProfileAsync(string userId, string? currentUserId);
        Task<IdentityResult> UpdateNotificationPreferencesAsync(string userId, NotificationPreferencesDto dto);
    }
}
