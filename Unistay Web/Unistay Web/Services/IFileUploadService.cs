using Microsoft.AspNetCore.Http;

namespace Unistay_Web.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadAvatarAsync(IFormFile file, string userId);
        Task<string> UploadCoverPhotoAsync(IFormFile file, string userId);
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }
}
