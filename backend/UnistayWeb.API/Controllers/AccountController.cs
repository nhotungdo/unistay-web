using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.DAL.Data;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UnistayWeb.API.ViewModels.LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                await LogLoginHistory(null, model.Email, false, "User not found");
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
            }

            if (user.IsBlocked)
            {
                await LogLoginHistory(user.Id, model.Email, false, $"Account blocked - Reason: {user.BlockReason}");
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản của bạn đã bị khóa." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _userManager.UpdateAsync(user);

                await LogLoginHistory(user.Id, model.Email, true, null, "Password");
                _logger.LogInformation("User logged in.");

                var token = await GenerateJwtToken(user);
                
                return Ok(new { 
                    token, 
                    user = new { 
                        id = user.Id, 
                        email = user.Email, 
                        fullName = user.FullName,
                        avatarUrl = user.AvatarUrl,
                        isOnboardingComplete = user.IsOnboardingComplete
                    } 
                });
            }
            if (result.IsLockedOut)
            {
                await LogLoginHistory(user.Id, model.Email, false, "Account locked - Too many failed attempts");
                _logger.LogWarning("User account locked out.");
                return StatusCode(StatusCodes.Status423Locked, new { message = "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần." });
            }
            else
            {
                await LogLoginHistory(user.Id, model.Email, false, "Invalid password");
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UnistayWeb.API.ViewModels.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new UserProfile
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.Phone,
                Provider = "Local",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                await _userManager.AddToRoleAsync(user, "Student");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // In API, we usually return success and send email in background
                _logger.LogInformation("Email confirmation token generated for {Email}", model.Email);
                await LogActivity(user.Id, "AccountCreated", "Created account as Student");

                var token = await GenerateJwtToken(user);

                return Ok(new { 
                    token, 
                    user = new { 
                        id = user.Id, 
                        email = user.Email, 
                        fullName = user.FullName,
                        isOnboardingComplete = user.IsOnboardingComplete
                    } 
                });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await LogLoginHistory(user.Id, user.Email, true, null, "Password", LogoutTime: DateTime.UtcNow);
            }

            // In JWT, logout is usually handled client-side by removing the token
            _logger.LogInformation("User logged out.");
            return Ok(new { message = "Đăng xuất thành công." });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new {
                id = user.Id,
                email = user.Email,
                fullName = user.FullName,
                phoneNumber = user.PhoneNumber,
                avatarUrl = user.AvatarUrl,
                isOnboardingComplete = user.IsOnboardingComplete,
                bio = user.Bio
            });
        }

        private async Task<string> GenerateJwtToken(UserProfile user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "YourSecretKeyHere_ChangeInProduction_MinimumLength32Characters"));
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

        private async Task LogLoginHistory(string? userId, string? email, bool isSuccessful, string? failureReason, string? authMethod = "Password", DateTime? LogoutTime = null)
        {
            try
            {
                var loginHistory = new LoginHistory
                {
                    UserId = userId ?? "",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    IsSuccessful = isSuccessful,
                    FailureReason = failureReason,
                    AuthenticationMethod = authMethod,
                    LoginTime = DateTime.UtcNow,
                    LogoutTime = LogoutTime
                };

                var userAgent = Request.Headers["User-Agent"].ToString();
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

        private async Task LogActivity(string userId, string activityType, string description)
        {
            try
            {
                var activity = new ActivityHistory
                {
                    UserId = userId,
                    ActivityType = activityType,
                    Description = description,
                    ActivityDate = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
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
    }
}
