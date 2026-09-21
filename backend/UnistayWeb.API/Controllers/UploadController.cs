using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.BLL.Services;

namespace UnistayWeb.API.Controllers.Api
{
    // DTOs to avoid Swashbuckle IFormFile issue
    public class UploadAvatarDto { public IFormFile? File { get; set; } }
    public class UploadCoverDto { public IFormFile? File { get; set; } }
    public class UploadAttachmentDto { public IFormFile? File { get; set; } }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UploadController> _logger;

        public UploadController(
            UserManager<UserProfile> userManager,
            IFileUploadService fileUploadService,
            IMemoryCache memoryCache,
            ILogger<UploadController> logger)
        {
            _userManager = userManager;
            _fileUploadService = fileUploadService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        private async Task<bool> CheckRateLimit(string userId)
        {
            var key = $"UploadLimit_{userId}";
            if (_memoryCache.TryGetValue(key, out int count))
            {
                if (count >= 10) // Limit: 10 uploads per hour
                {
                    return false;
                }
                _memoryCache.Set(key, count + 1, TimeSpan.FromHours(1));
            }
            else
            {
                _memoryCache.Set(key, 1, TimeSpan.FromHours(1));
            }
            return true;
        }

        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (!await CheckRateLimit(user.Id))
                {
                    return StatusCode(429, new { success = false, message = "Bạn đã tải lên quá nhiều lần. Vui lòng thử lại sau." });
                }

                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new { success = false, message = "Vui lòng chọn file." });

                var url = await _fileUploadService.UploadAvatarAsync(dto.File, user.Id);

                user.AvatarUrl = url;
                await _userManager.UpdateAsync(user);

                return Ok(new { success = true, url, message = "Cập nhật ảnh đại diện thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi tải ảnh." });
            }
        }

        [HttpPost("cover")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCover([FromForm] UploadCoverDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (!await CheckRateLimit(user.Id))
                {
                    return StatusCode(429, new { success = false, message = "Bạn đã tải lên quá nhiều lần. Vui lòng thử lại sau." });
                }

                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new { success = false, message = "Vui lòng chọn file." });

                var url = await _fileUploadService.UploadCoverPhotoAsync(dto.File, user.Id);

                user.CoverPhotoUrl = url;
                await _userManager.UpdateAsync(user);

                return Ok(new { success = true, url, message = "Cập nhật ảnh bìa thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading cover photo");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi tải ảnh bìa." });
            }
        }

        [HttpPost("message-attachment")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMessageAttachment([FromForm] UploadAttachmentDto dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest(new { success = false, message = "Vui lòng chọn file." });

                var url = await _fileUploadService.UploadFileAsync(dto.File, "chat-attachments");
                return Ok(new { success = true, url, fileName = dto.File.FileName, fileSize = dto.File.Length });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading message attachment");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi tải tệp." });
            }
        }
    }
}
