using Microsoft.AspNetCore.Identity;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ILogger<OnboardingService> _logger;

        public OnboardingService(
            UserManager<UserProfile> userManager,
            ILogger<OnboardingService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<OnboardingStatusDto> GetStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            return new OnboardingStatusDto
            {
                IsOnboardingComplete = user.IsOnboardingComplete,
                OnboardingStep = user.OnboardingStep
            };
        }

        public async Task<OnboardingResultDto> SelectRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (role == "Landlord")
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

            return new OnboardingResultDto
            {
                Message = "Cập nhật vai trò thành công",
                NextStep = 2
            };
        }

        public async Task<OnboardingResultDto> UpdateSeekerProfileAsync(string userId, SeekerProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.DateOfBirth = dto.DateOfBirth;
            user.ZodiacSign = CalculateZodiac(dto.DateOfBirth);
            user.Lifestyle = dto.Preferences;
            user.CompatibilityAnalysis = AnalyzeCompatibility(user.ZodiacSign, dto.Preferences);
            user.OnboardingStep = 3;

            await _userManager.UpdateAsync(user);

            return new OnboardingResultDto
            {
                Message = "Cập nhật hồ sơ thành công",
                NextStep = 3,
                Analysis = user.CompatibilityAnalysis
            };
        }

        public async Task<OnboardingResultDto> CompleteOnboardingAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.IsOnboardingComplete = true;
            await _userManager.UpdateAsync(user);

            return new OnboardingResultDto
            {
                Message = "Hoàn tất onboarding"
            };
        }

        public async Task<OnboardingResultDto> LandlordVerifyAsync(string userId, LandlordVerifyDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.PhoneNumber = dto.Phone;
            user.LivingArea = dto.Address;
            user.HouseNumber = dto.HouseNumber;
            user.StreetName = dto.StreetName;
            user.City = dto.City;
            user.District = dto.District;
            user.Ward = dto.Ward;
            user.OnboardingStep = 3;

            await _userManager.UpdateAsync(user);

            return new OnboardingResultDto
            {
                Message = "Xác thực chủ trọ thành công",
                NextStep = 3
            };
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
