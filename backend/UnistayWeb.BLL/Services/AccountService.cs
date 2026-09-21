using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            IConfiguration configuration,
            ILogger<AccountService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResultDto> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                await LogLoginHistoryAsync(null, email, false, "User not found");
                return new LoginResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Email hoặc mật khẩu không đúng.",
                    HttpStatusCode = 401
                };
            }

            if (user.IsBlocked)
            {
                await LogLoginHistoryAsync(user.Id, email, false, $"Account blocked - Reason: {user.BlockReason}");
                return new LoginResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Tài khoản của bạn đã bị khóa.",
                    HttpStatusCode = 403
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = GetIpAddress();
                await _userManager.UpdateAsync(user);

                await LogLoginHistoryAsync(user.Id, email, true, null, "Password");
                _logger.LogInformation("User logged in.");

                var token = await GenerateJwtTokenAsync(user);

                return new LoginResultDto
                {
                    IsSuccess = true,
                    Response = new AuthResponseDto
                    {
                        Token = token,
                        User = new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email,
                            FullName = user.FullName,
                            AvatarUrl = user.AvatarUrl,
                            IsOnboardingComplete = user.IsOnboardingComplete
                        }
                    }
                };
            }

            if (result.IsLockedOut)
            {
                await LogLoginHistoryAsync(user.Id, email, false, "Account locked - Too many failed attempts");
                _logger.LogWarning("User account locked out.");
                return new LoginResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần.",
                    HttpStatusCode = 423
                };
            }
            else
            {
                await LogLoginHistoryAsync(user.Id, email, false, "Invalid password");
                return new LoginResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Email hoặc mật khẩu không đúng.",
                    HttpStatusCode = 401
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(string email, string fullName, string phone, string password)
        {
            var user = new UserProfile
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phone,
                Provider = "Local",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                await _userManager.AddToRoleAsync(user, "Student");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("Email confirmation token generated for {Email}", email);

                await LogActivityAsync(user.Id, "AccountCreated", "Created account as Student");

                var token = await GenerateJwtTokenAsync(user);

                return new AuthResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        IsOnboardingComplete = user.IsOnboardingComplete
                    }
                };
            }

            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        public async Task LogoutAsync(string userId, string? email)
        {
            await LogLoginHistoryAsync(userId, email, true, null, "Password", DateTime.UtcNow);
            _logger.LogInformation("User logged out.");
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                IsOnboardingComplete = user.IsOnboardingComplete,
                Bio = user.Bio
            };
        }

        public async Task LogLoginHistoryAsync(string? userId, string? email, bool isSuccessful, string? failureReason, string? authMethod = "Password", DateTime? logoutTime = null)
        {
            try
            {
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;

                var loginHistory = new LoginHistory
                {
                    UserId = userId ?? "",
                    IpAddress = GetIpAddress(),
                    UserAgent = userAgent,
                    IsSuccessful = isSuccessful,
                    FailureReason = failureReason,
                    AuthenticationMethod = authMethod,
                    LoginTime = DateTime.UtcNow,
                    LogoutTime = logoutTime
                };

                loginHistory.Browser = ParseBrowser(userAgent);
                loginHistory.OperatingSystem = ParseOperatingSystem(userAgent);
                loginHistory.DeviceType = ParseDeviceType(userAgent);

                _context.LoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging login history for {Email}", email);
            }
        }

        public async Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress = null)
        {
            try
            {
                var activity = new ActivityHistory
                {
                    UserId = userId,
                    ActivityType = activityType,
                    Description = description,
                    ActivityDate = DateTime.UtcNow,
                    IpAddress = ipAddress ?? GetIpAddress(),
                    IsPublic = false
                };

                _context.ActivityHistories.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging activity for user {UserId}", userId);
            }
        }

        private async Task<string> GenerateJwtTokenAsync(UserProfile user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSecretKeyHere_ChangeInProduction_MinimumLength32Characters";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ParseBrowser(string userAgent)
        {
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            return "Unknown";
        }

        private string ParseOperatingSystem(string userAgent)
        {
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
            return "Unknown";
        }

        private string ParseDeviceType(string userAgent)
        {
            if (userAgent.Contains("Mobile") || userAgent.Contains("Android")) return "Mobile";
            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad")) return "Tablet";
            return "Desktop";
        }

        private string? GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }
}
