using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ApplicationDbContext _context;

        public OnboardingController(UserManager<UserProfile> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            return Ok(new
            {
                isOnboardingComplete = user.IsOnboardingComplete,
                onboardingStep = user.OnboardingStep
            });
        }

        public class RoleSelectionDto
        {
            public string Role { get; set; } = string.Empty;
        }

        [HttpPost("select-role")]
        public async Task<IActionResult> SelectRole([FromBody] RoleSelectionDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (model.Role == "Landlord")
            {
                await _userManager.RemoveFromRoleAsync(user, "Student");
                await _userManager.AddToRoleAsync(user, "Landlord");
            }
            else
            {
                if (!await _userManager.IsInRoleAsync(user, "Student"))
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }
            }

            user.OnboardingStep = 2;
            await _userManager.UpdateAsync(user);
            
            return Ok(new { message = "Cập nhật vai trò thành công", nextStep = 2 });
        }

        public class SeekerProfileDto
        {
            public DateTime DateOfBirth { get; set; }
            public string Preferences { get; set; } = string.Empty;
        }

        [HttpPost("seeker-profile")]
        public async Task<IActionResult> SeekerProfile([FromBody] SeekerProfileDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.DateOfBirth = model.DateOfBirth;
            user.ZodiacSign = CalculateZodiac(model.DateOfBirth);
            user.Lifestyle = model.Preferences;
            user.CompatibilityAnalysis = AnalyzeCompatibility(user.ZodiacSign, model.Preferences);
            user.OnboardingStep = 3;
            
            await _userManager.UpdateAsync(user);

            return Ok(new 
            { 
                message = "Cập nhật hồ sơ thành công", 
                nextStep = 3,
                analysis = user.CompatibilityAnalysis
            });
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteOnboarding()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.IsOnboardingComplete = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Hoàn tất onboarding" });
        }

        public class LandlordVerifyDto
        {
            public string Phone { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string HouseNumber { get; set; } = string.Empty;
            public string StreetName { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string District { get; set; } = string.Empty;
            public string Ward { get; set; } = string.Empty;
        }

        [HttpPost("landlord-verify")]
        public async Task<IActionResult> LandlordVerify([FromBody] LandlordVerifyDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.PhoneNumber = model.Phone;
            user.LivingArea = model.Address;
            user.HouseNumber = model.HouseNumber;
            user.StreetName = model.StreetName;
            user.City = model.City;
            user.District = model.District;
            user.Ward = model.Ward;
            
            user.OnboardingStep = 3;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Xác thực chủ trọ thành công", nextStep = 3 });
        }

        private string CalculateZodiac(DateTime dob)
        {
            int day = dob.Day;
            int month = dob.Month;
            
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
            if ((month == 2 && day >= 19) || (month == 3 && day <= 20)) return "Pisces";
            
            return "Unknown";
        }

        private string AnalyzeCompatibility(string zodiac, string preferences)
        {
            var comp = zodiac switch
            {
                "Aries" => "Leo, Sagittarius, Gemini, Aquarius",
                "Taurus" => "Virgo, Capricorn, Cancer, Pisces",
                "Gemini" => "Libra, Aquarius, Aries, Leo",
                "Cancer" => "Scorpio, Pisces, Taurus, Virgo",
                "Leo" => "Aries, Sagittarius, Gemini, Libra",
                "Virgo" => "Taurus, Capricorn, Cancer, Scorpio",
                "Libra" => "Gemini, Aquarius, Leo, Sagittarius",
                "Scorpio" => "Cancer, Pisces, Virgo, Capricorn",
                "Sagittarius" => "Aries, Leo, Libra, Aquarius",
                "Capricorn" => "Taurus, Virgo, Scorpio, Pisces",
                "Aquarius" => "Gemini, Libra, Sagittarius, Aries",
                "Pisces" => "Cancer, Scorpio, Taurus, Capricorn",
                _ => "All signs"
            };

            return $"Based on your sign ({zodiac}), your most compatible roommates are likely {comp}. Your preference for '{preferences}' matches well with stable earth signs or dynamic fire signs depending on your specific chart.";
        }
    }
}
