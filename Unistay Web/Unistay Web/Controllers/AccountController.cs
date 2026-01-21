using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Unistay_Web.Models.User;
using Unistay_Web.Data;

namespace Unistay_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Unistay_Web.ViewModels.LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                await LogLoginHistory(null, model.Email, false, "User not found");
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                await LogLoginHistory(user.Id, model.Email, false, $"Account blocked - Reason: {user.BlockReason}");
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _userManager.UpdateAsync(user);

                await LogLoginHistory(user.Id, model.Email, true, null, "Password");

                _logger.LogInformation("User logged in.");

                if (!user.IsOnboardingComplete)
                {
                    return RedirectToAction("Index", "Onboarding");
                }

                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                await LogLoginHistory(user.Id, model.Email, false, "Account locked - Too many failed attempts");
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần.");
                return View(model);
            }
            else
            {
                await LogLoginHistory(user.Id, model.Email, false, "Invalid password");
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Unistay_Web.ViewModels.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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

                // Assign default Student role
                await _userManager.AddToRoleAsync(user, "Student");

                // Generate email confirmation token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

                // TODO: Send email with confirmation link
                _logger.LogInformation("Email confirmation link sent to {Email}", model.Email);

                // Log registration activity
                await LogActivity(user.Id, "AccountCreated", "Created account as Student");

                // Auto sign in after registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Onboarding");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                user.EmailVerifiedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await LogActivity(user.Id, "EmailVerified", "Email address verified");
                _logger.LogInformation("User {UserId} confirmed email.", userId);
                ViewBag.SuccessMessage = "Email đã được xác thực thành công!";
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi xác thực email. Vui lòng thử lại.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information.");
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _userManager.UpdateAsync(user);
                    await LogLoginHistory(user.Id, user.Email, true, null, "Google");
                }

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                
                if (!user.IsOnboardingComplete)
                {
                    return RedirectToAction("Index", "Onboarding");
                }

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                // If the user does not have an account, create one
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var picture = info.Principal.FindFirstValue("picture");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email not provided by Google.");
                    return RedirectToAction(nameof(Login));
                }

                var user = new UserProfile
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    Provider = "Google",
                    ProviderKey = info.ProviderKey,
                    AvatarUrl = picture,
                    EmailConfirmed = true, // Google emails are already verified
                    EmailVerifiedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);

                if (createResult.Succeeded)
                {
                    // Add default role
                    await _userManager.AddToRoleAsync(user, "Student");

                    createResult = await _userManager.AddLoginAsync(user, info);
                    if (createResult.Succeeded)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                        await _userManager.UpdateAsync(user);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await LogLoginHistory(user.Id, email, true, null, "Google");
                        await LogActivity(user.Id, "AccountCreated", "Created account via Google OAuth");

                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToAction("Index", "Onboarding");
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    _logger.LogError("Error creating user: {Error}", error.Description);
                }

                return RedirectToAction(nameof(Login));
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập email.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, protocol: Request.Scheme);

            // TODO: Send email with callbackUrl
            _logger.LogInformation("Password reset token generated for {Email}", email);
            await LogActivity(user.Id, "PasswordResetRequested", "Requested password reset");

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token = null)
        {
            if (token == null)
            {
                return BadRequest("Token is missing");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string token, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);

            if (result.Succeeded)
            {
                await LogActivity(user.Id, "PasswordReset", "Reset password");
                _logger.LogInformation("User {UserId} reset password.", user.Id);
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await LogLoginHistory(user.Id, user.Email, true, null, "Password", LogoutTime: DateTime.UtcNow);
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Helper method to log login history
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

                // Parse user agent for device info (simplified)
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

        // Helper method to log activity
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
