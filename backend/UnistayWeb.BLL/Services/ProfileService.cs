using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Connection;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProfileService> logger)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var activityHistory = await _context.ActivityHistories
                .Where(a => a.UserId == user.Id && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            return new ProfileDto
            {
                UserProfile = user,
                ActivityHistory = activityHistory
            };
        }

        public async Task<IdentityResult> EditProfileAsync(string userId, EditProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

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
                user.ZodiacSign = CalculateZodiac(dto.DateOfBirth.Value);
            }

            user.City = dto.City;
            user.District = dto.District;
            user.Ward = dto.Ward;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivityAsync(user.Id, "ProfileUpdate", "Updated profile information");
            }

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (result.Succeeded)
            {
                await LogActivityAsync(user.Id, "PasswordChanged", "Changed account password");
            }

            return result;
        }

        public async Task<PagedHistoryDto<LoginHistory>> GetLoginHistoryAsync(string userId, int page)
        {
            var pageSize = 20;
            var query = _context.LoginHistories
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LoginTime);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasNextPage = items.Count > pageSize;
            if (hasNextPage)
            {
                items = items.Take(pageSize).ToList();
            }

            return new PagedHistoryDto<LoginHistory>
            {
                Data = items,
                CurrentPage = page,
                HasNextPage = hasNextPage
            };
        }

        public async Task<PagedHistoryDto<ActivityHistory>> GetActivityHistoryAsync(string userId, int page)
        {
            var pageSize = 20;
            var query = _context.ActivityHistories
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ActivityDate);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasNextPage = items.Count > pageSize;
            if (hasNextPage)
            {
                items = items.Take(pageSize).ToList();
            }

            return new PagedHistoryDto<ActivityHistory>
            {
                Data = items,
                CurrentPage = page,
                HasNextPage = hasNextPage
            };
        }

        public async Task<bool> VerifyEmailAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.EmailConfirmed)
                return false;

            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("Email verification code generated for {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email verification token for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<PublicProfileDto> GetPublicProfileAsync(string userId, string? currentUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive || user.IsBlocked)
                throw new InvalidOperationException("User not found");

            var activities = await _context.ActivityHistories
                .Where(a => a.UserId == userId && a.IsPublic)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            var publicProfileDto = new PublicProfileDto
            {
                UserProfile = user,
                Activities = activities
            };

            if (currentUserId != null)
            {
                if (currentUserId == userId)
                {
                    publicProfileDto.ConnectionStatus = "Self";
                    publicProfileDto.ConnectionId = 0;
                }
                else
                {
                    var connection = await _context.Connections
                        .FirstOrDefaultAsync(c => (c.RequesterId == currentUserId && c.AddresseeId == userId) ||
                                                   (c.RequesterId == userId && c.AddresseeId == currentUserId));

                    if (connection != null)
                    {
                        publicProfileDto.ConnectionId = connection.Id;
                        if (connection.Status == ConnectionStatus.Accepted)
                            publicProfileDto.ConnectionStatus = "Accepted";
                        else if (connection.Status == ConnectionStatus.Pending)
                        {
                            publicProfileDto.ConnectionStatus = connection.RequesterId == currentUserId ? "Pending_Sent" : "Pending_Received";
                        }
                        else if (connection.Status == ConnectionStatus.Declined)
                        {
                            publicProfileDto.ConnectionStatus = "Declined";
                        }
                    }
                }
            }

            return publicProfileDto;
        }

        public async Task<IdentityResult> UpdateNotificationPreferencesAsync(string userId, NotificationPreferencesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.NotificationEmailEnabled = dto.EmailEnabled;
            user.NotificationSmsEnabled = dto.SmsEnabled;
            user.NotificationPushEnabled = dto.PushEnabled;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await LogActivityAsync(user.Id, "NotificationPreferencesUpdated", "Updated notification preferences");
            }

            return result;
        }

        private async Task LogActivityAsync(string userId, string activityType, string description)
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

                var activity = new ActivityHistory
                {
                    UserId = userId,
                    ActivityType = activityType,
                    Description = description,
                    ActivityDate = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    IsPublic = true
                };

                _context.ActivityHistories.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging activity for user {UserId}", userId);
            }
        }

        private static string CalculateZodiac(DateTime dateOfBirth)
        {
            int day = dateOfBirth.Day;
            int month = dateOfBirth.Month;

            if ((month == 3 && day >= 21) || (month == 4 && day <= 19)) return "Aries";
            if ((month == 4 && day >= 20) || (month == 5 && day <= 20)) return "Taurus";
            if ((month == 5 && day >= 21) || (month == 6 && day <= 20)) return "Gemini";
            if ((month == 6 && day >= 21) || (month == 7 && day <= 22)) return "Cancer";
            if ((month == 7 && day >= 23) || (month == 8 && day <= 22)) return "Leo";
            if ((month == 8 && day >= 23) || (month == 9 && day <= 22)) return "Virgo";
            if ((month == 9 && day >= 23) || (month == 10 && day <= 22)) return "Libra";
            if ((month == 10 && day >= 23) || (month == 11 && day <= 21)) return "Scorpio";
            if ((month == 11 && day >= 22) || (month == 12 && day <= 21)) return "Sagittarius";
            if ((month == 12 && day >= 22) || (month == 1 && day <= 19)) return "Capricorn";
            if ((month == 1 && day >= 20) || (month == 2 && day <= 18)) return "Aquarius";
            return "Pisces";
        }
    }
}
