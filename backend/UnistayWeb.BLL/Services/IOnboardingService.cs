using UnistayWeb.BLL.ViewModels;

namespace UnistayWeb.BLL.Services
{
    public interface IOnboardingService
    {
        Task<OnboardingStatusDto> GetStatusAsync(string userId);
        Task<OnboardingResultDto> SelectRoleAsync(string userId, string role);
        Task<OnboardingResultDto> UpdateSeekerProfileAsync(string userId, SeekerProfileDto dto);
        Task<OnboardingResultDto> CompleteOnboardingAsync(string userId);
        Task<OnboardingResultDto> LandlordVerifyAsync(string userId, LandlordVerifyDto dto);
    }
}
