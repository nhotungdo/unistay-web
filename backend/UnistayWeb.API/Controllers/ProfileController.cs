using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.DAL.Data;
using UnistayWeb.API.Helpers;
using UnistayWeb.DAL.Models.Connection;

namespace UnistayWeb.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<UserProfile> userManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var activityHistory = await _context.ActivityHistories
                .Where(a => a.UserId == user.Id && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            return Ok(new {
                user = new {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Gender,
                    user.Age,
                    user.Occupation,
                    user.Bio,
                    user.LivingArea,
                    user.Budget,
                    user.Lifestyle,
                    user.ExpectedStayDuration,
                    user.DateOfBirth,
                    user.ZodiacSign,
                    user.City,
                    user.District,
                    user.Ward,
                    user.AvatarUrl,
                    user.NotificationEmailEnabled,
                    user.NotificationSmsEnabled,
                    user.NotificationPushEnabled
                },
                activityHistory
            });
        }

        public class EditProfileDto
        {
            public string FullName { get; set; } = string.Empty;
            public string? Gender { get; set; }
            public int? Age { get; set; }
            public string? Occupation { get; set; }
            public string? Bio { get; set; }
            public string? LivingArea { get; set; }
            public decimal? Budget { get; set; }
            public string? Lifestyle { get; set; }
            public DateTime? ExpectedStayDuration { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? City { get; set; }
            public string? District { get; set; }
            public string? Ward { get; set; }
            public string? PhoneNumber { get; set; }
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrEmpty(dto.FullName))
            {
                return BadRequest(new { message = "Tên đầy đủ không được để trống." });
            }

            user.FullName = dto.FullName;
            user.Gender = dto.Gender;
            user.Age = dto.Age;
            user.Occupation = dto.Occupation;
            user.Bio = dto.Bio;
            user.LivingArea = dto.LivingArea;
            user.Budget = dto.Budget;
            user.Lifestyle = dto.Lifestyle;
            user.ExpectedStayDuration = dto.ExpectedStayDuration;
            
            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = dto.DateOfBirth;
                var zodiacInfo = ZodiacHelper.GetZodiacInfo(dto.DateOfBirth.Value);
                user.ZodiacSign = zodiacInfo.Name;
            }

            user.City = dto.City;
            user.District = dto.District;
            user.Ward = dto.Ward;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "ProfileUpdate", $"Updated profile information");
                return Ok(new { message = "Cập nhật hồ sơ thành công!" });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // POST: api/Profile/ChangePassword
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrEmpty(dto.CurrentPassword) || string.IsNullOrEmpty(dto.NewPassword))
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin." });
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "Mật khẩu mới không khớp." });
            }

            if (dto.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 8 ký tự." });
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "PasswordChanged", "Changed account password");
                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // GET: api/Profile/LoginHistory
        [HttpGet("LoginHistory")]
        public async Task<IActionResult> LoginHistory([FromQuery] int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            const int pageSize = 20;
            var loginHistories = await _context.LoginHistories
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasNextPage = loginHistories.Count > pageSize;
            if (hasNextPage)
            {
                loginHistories = loginHistories.Take(pageSize).ToList();
            }

            return Ok(new { data = loginHistories, currentPage = page, hasNextPage });
        }

        // GET: api/Profile/ActivityHistory
        [HttpGet("ActivityHistory")]
        public async Task<IActionResult> ActivityHistory([FromQuery] int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            const int pageSize = 20;
            var activities = await _context.ActivityHistories
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.ActivityDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasNextPage = activities.Count > pageSize;
            if (hasNextPage)
            {
                activities = activities.Take(pageSize).ToList();
            }

            return Ok(new { data = activities, currentPage = page, hasNextPage });
        }

        // POST: api/Profile/VerifyEmail
        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.EmailConfirmed)
            {
                return BadRequest(new { message = "Email đã được xác thực." });
            }

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // TODO: Send email with verification link
                _logger.LogInformation("Email verification code generated for {Email}", user.Email);

                return Ok(new { message = "Vui lòng kiểm tra email để xác thực." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email verification token for user {UserId}", user.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi: " + ex.Message });
            }
        }

        // GET: api/Profile/PublicProfile/{userId}
        [AllowAnonymous]
        [HttpGet("PublicProfile/{userId}")]
        public async Task<IActionResult> PublicProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive || user.IsBlocked)
            {
                return NotFound();
            }

            var activities = await _context.ActivityHistories
                .Where(a => a.UserId == userId && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            var currentUserId = _userManager.GetUserId(User);
            string status = "None";
            int connectionId = 0;

            if (currentUserId != null)
            {
                if (currentUserId == userId)
                {
                    status = "Self";
                }
                else
                {
                    var connection = await _context.Connections
                        .FirstOrDefaultAsync(c => (c.RequesterId == currentUserId && c.AddresseeId == userId) ||
                                                  (c.RequesterId == userId && c.AddresseeId == currentUserId));
                    
                    if (connection != null)
                    {
                        connectionId = connection.Id;
                        if (connection.Status == ConnectionStatus.Accepted) status = "Accepted";
                        else if (connection.Status == ConnectionStatus.Pending)
                        {
                            status = connection.RequesterId == currentUserId ? "Pending_Sent" : "Pending_Received";
                        }
                        else if (connection.Status == ConnectionStatus.Declined)
                        {
                            status = "Declined";
                        }
                    }
                }
            }

            return Ok(new {
                user = new {
                    user.Id,
                    user.FullName,
                    user.AvatarUrl,
                    user.Bio,
                    user.Gender,
                    user.Age,
                    user.Occupation,
                    user.City,
                    user.District,
                    user.ZodiacSign
                },
                activities,
                connectionStatus = status,
                connectionId
            });
        }

        public class NotificationPreferencesDto
        {
            public bool EmailEnabled { get; set; }
            public bool SmsEnabled { get; set; }
            public bool PushEnabled { get; set; }
        }

        // POST: api/Profile/UpdateNotificationPreferences
        [HttpPost("UpdateNotificationPreferences")]
        public async Task<IActionResult> UpdateNotificationPreferences([FromBody] NotificationPreferencesDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.NotificationEmailEnabled = dto.EmailEnabled;
            user.NotificationSmsEnabled = dto.SmsEnabled;
            user.NotificationPushEnabled = dto.PushEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "NotificationPreferencesUpdated", "Updated notification preferences");
                return Ok(new { message = "Cập nhật tùy chọn thông báo thành công." });
            }

            return BadRequest(new { message = "Lỗi cập nhật tùy chọn." });
        }

        private async Task LogActivity(string userId, string activityType, string description, string? relatedEntity = null, string? relatedEntityType = null)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var activity = new ActivityHistory
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                ActivityDate = DateTime.UtcNow,
                IpAddress = ipAddress,
                RelatedEntity = relatedEntity,
                RelatedEntityType = relatedEntityType,
                IsPublic = true
            };

            _context.ActivityHistories.Add(activity);
            await _context.SaveChangesAsync();
        }
    }
}
