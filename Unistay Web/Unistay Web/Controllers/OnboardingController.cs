using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Unistay_Web.Data;
using Unistay_Web.Models.User;
using System.Security.Claims;

namespace Unistay_Web.Controllers
{
    [Authorize]
    public class OnboardingController : Controller
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ApplicationDbContext _context;

        public OnboardingController(UserManager<UserProfile> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.IsOnboardingComplete)
            {
                return RedirectToAction("Index", "Home");
            }

            // Route based on step
            return user.OnboardingStep switch
            {
                1 => RedirectToAction("SelectRole"),
                2 => await _userManager.IsInRoleAsync(user, "Landlord") ? RedirectToAction("LandlordVerify") : RedirectToAction("SeekerProfile"),
                3 => await _userManager.IsInRoleAsync(user, "Landlord") ? RedirectToAction("LandlordReview") : RedirectToAction("SeekerResult"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public IActionResult SelectRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SelectRole(string role)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (role == "Landlord")
            {
                await _userManager.RemoveFromRoleAsync(user, "Student");
                await _userManager.AddToRoleAsync(user, "Landlord");
            }
            else
            {
                // Ensure Student role
                if (!await _userManager.IsInRoleAsync(user, "Student"))
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }
            }

            user.OnboardingStep = 2;
            await _userManager.UpdateAsync(user);
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult SeekerProfile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeekerProfile(DateTime dob, string preferences, string submitAction)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.DateOfBirth = dob;
            user.ZodiacSign = CalculateZodiac(dob);
            user.Lifestyle = preferences; // Using Lifestyle field for preferences for now
            
            // "AI" Analysis simulation
            user.CompatibilityAnalysis = AnalyzeCompatibility(user.ZodiacSign, preferences);
            
            if (submitAction == "next")
            {
                user.OnboardingStep = 3;
            }
            
            await _userManager.UpdateAsync(user);

            if (submitAction == "save")
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("SeekerResult");
        }

        [HttpGet]
        public async Task<IActionResult> SeekerResult()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOnboarding()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.IsOnboardingComplete = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Home");
        }

        // Landlord Logic
        [HttpGet]
        public IActionResult LandlordVerify()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LandlordVerify(string address, string phone, string houseNumber, string streetName, string city, string district, string ward, string submitAction)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.PhoneNumber = phone;
            // In a real app, verify OTP here.
            
            user.LivingArea = address; // Using LivingArea to store full address string
            user.HouseNumber = houseNumber;
            user.StreetName = streetName;
            user.City = city;
            user.District = district;
            user.Ward = ward;
            
            if (submitAction == "next")
            {
                user.OnboardingStep = 3;
            }

            await _userManager.UpdateAsync(user);

            if (submitAction == "save")
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("LandlordReview");
        }

        [HttpGet]
        public async Task<IActionResult> LandlordReview()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // Helpers
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
            // Simple mock logic
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
