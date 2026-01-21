using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Unistay_Web.Services
{
    public class LocalFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private const int AVATAR_WIDTH = 200;
        private const int AVATAR_HEIGHT = 200;
        private const int COVER_WIDTH = 1200;
        private const int COVER_HEIGHT = 675;

        public LocalFileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAvatarAsync(IFormFile file, string userId)
        {
            return await ProcessAndSaveImageAsync(file, userId, "avatars", AVATAR_WIDTH, AVATAR_HEIGHT);
        }

        public async Task<string> UploadCoverPhotoAsync(IFormFile file, string userId)
        {
            return await ProcessAndSaveImageAsync(file, userId, "covers", COVER_WIDTH, COVER_HEIGHT);
        }

        private async Task<string> ProcessAndSaveImageAsync(IFormFile file, string userId, string folder, int width, int height)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");
            
            if (file.Length > 5 * 1024 * 1024) // 5MB
                throw new ArgumentException("File size exceeds 5MB limit");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file format");

            // Basic Magic Number Check (Security)
            if (!IsValidImageHeader(file))
               throw new ArgumentException("Invalid image file content");

            // Create directory
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate filename unique to user but avoiding collisions
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = file.OpenReadStream())
            using (var image = await Image.LoadAsync(stream))
            {
                // Resize/Crop
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Crop
                }));

                // Save
                await image.SaveAsync(filePath); 
            }

            return $"/uploads/{folder}/{fileName}";
        }

        private bool IsValidImageHeader(IFormFile file)
        {
            // Simple check for common image headers
             try
            {
                using (var stream = file.OpenReadStream())
                {
                    var header = new byte[8];
                    stream.Read(header, 0, 8);
                    
                    // JPG: FF D8 FF
                    if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
                    // PNG: 89 50 4E 47
                    if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
                    // GIF: 47 49 46
                    if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46) return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
