using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.Roommate;

namespace UnistayWeb.BLL.Services
{
    public interface IRoommateService
    {
        Task<List<RoommateDisplayDto>> GetRoommatesAsync(string? searchString, string? gender, string? budgetRange);
        Task<RoommateProfileDetailViewModel?> GetProfileAsync(int id, string priority, string? currentUserId);
        Task CreateProfileAsync(CreateRoommateDto dto, string userId, string webRootPath);
        Task<List<RoommateDisplayDto>> MatchAsync(string userId);
    }
}
