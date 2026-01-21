using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Models.User;
using Unistay_Web.Data;
using System.Security.Claims;
using Unistay_Web.Helpers;

namespace Unistay_Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProfileController(
            UserManager<UserProfile> userManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        // GET: /Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var activityHistory = await _context.ActivityHistories
                .Where(a => a.UserId == user.Id && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            ViewBag.ActivityHistory = activityHistory;
            return View(user);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string fullName, string? gender, int? age, string? occupation,
            string? bio, string? livingArea, decimal? budget, string? lifestyle,
            DateTime? expectedStayDuration, DateTime? dateOfBirth, string? city, string? district, string? ward, string? phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError(string.Empty, "Tên đầy đủ không được để trống.");
                return View(user);
            }

            // Update user profile information
            user.FullName = fullName;
            user.Gender = gender;
            user.Age = age;
            user.Occupation = occupation;
            user.Bio = bio;
            user.LivingArea = livingArea;
            user.Budget = budget;
            user.Lifestyle = lifestyle;
            user.ExpectedStayDuration = expectedStayDuration;
            
            // Calculate Zodiac if DOB changed
            if (dateOfBirth.HasValue)
            {
                user.DateOfBirth = dateOfBirth;
                var zodiacInfo = ZodiacHelper.GetZodiacInfo(dateOfBirth.Value);
                user.ZodiacSign = zodiacInfo.Name;
            }

            user.City = city;
            user.District = district;
            user.Ward = ward;
            user.PhoneNumber = phoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Log activity
                await LogActivity(user.Id, "ProfileUpdate", $"Updated profile information");

                _logger.LogInformation("User {UserId} updated profile.", user.Id);
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        // Deprecated: Use API/UploadController instead
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        // { ... }

        // GET: /Profile/ChangePassword
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới không khớp.");
                return View();
            }

            if (newPassword.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới phải có ít nhất 8 ký tự.");
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "PasswordChanged", "Changed account password");
                _logger.LogInformation("User {UserId} changed password.", user.Id);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        // GET: /Profile/LoginHistory
        [HttpGet]
        public async Task<IActionResult> LoginHistory(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

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

            ViewBag.CurrentPage = page;
            ViewBag.HasNextPage = hasNextPage;
            return View(loginHistories);
        }

        // GET: /Profile/ActivityHistory
        [HttpGet]
        public async Task<IActionResult> ActivityHistory(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

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

            ViewBag.CurrentPage = page;
            ViewBag.HasNextPage = hasNextPage;
            return View(activities);
        }

        // POST: /Profile/VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            if (user.EmailConfirmed)
            {
                return Json(new { success = false, message = "Email đã được xác thực." });
            }

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

                // TODO: Send email with verification link
                _logger.LogInformation("Email verification code generated for {Email}", user.Email);

                return Json(new { success = true, message = "Vui lòng kiểm tra email để xác thực." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email verification token for user {UserId}", user.Id);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: /Profile/PublicProfile/:userId
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PublicProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive || user.IsBlocked)
            {
                return NotFound();
            }

            // Get public activity history
            var activities = await _context.ActivityHistories
                .Where(a => a.UserId == userId && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Activities = activities;
            return View(user);
        }

        // POST: /Profile/UpdateNotificationPreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotificationPreferences(bool emailEnabled, bool smsEnabled, bool pushEnabled)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            user.NotificationEmailEnabled = emailEnabled;
            user.NotificationSmsEnabled = smsEnabled;
            user.NotificationPushEnabled = pushEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "NotificationPreferencesUpdated", "Updated notification preferences");
                return Json(new { success = true, message = "Cập nhật tùy chọn thông báo thành công." });
            }

            return Json(new { success = false, message = "Lỗi cập nhật tùy chọn." });
        }

        // Helper method to log user activity
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
