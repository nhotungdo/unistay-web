using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Roommate;
using Unistay_Web.Models.User;
using Unistay_Web.Models.Connection;
using Microsoft.AspNetCore.Authorization;

namespace Unistay_Web.Controllers
{
    public class RoommatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Unistay_Web.Services.IAiMatchingService _aiMatchingService;

        public RoommatesController(ApplicationDbContext context, UserManager<UserProfile> userManager, IWebHostEnvironment webHostEnvironment, Unistay_Web.Services.IAiMatchingService aiMatchingService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _aiMatchingService = aiMatchingService;
        }

        public async Task<IActionResult> Index()
        {
            var profiles = await _context.RoommateProfiles.OrderByDescending(p => p.CreatedAt).ToListAsync();
            var userIds = profiles.Select(p => p.UserId).Distinct().ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            var viewModels = profiles.Select(p =>
            {
                users.TryGetValue(p.UserId, out var user);
                int age = 0;
                if (user?.DateOfBirth.HasValue == true)
                {
                    age = DateTime.Now.Year - user.DateOfBirth.Value.Year;
                }

                return new RoommateDisplayDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    FullName = user?.FullName ?? "Người dùng",
                    AvatarUrl = user?.AvatarUrl,
                    Age = age,
                    Gender = p.Gender ?? user?.Gender,
                    Occupation = user?.Occupation,
                    Budget = p.Budget,
                    PreferredArea = p.PreferredArea,
                    Habits = p.Habits?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                    MatchPercentage = new Random(p.Id).Next(75, 99),
                    CreatedAt = p.CreatedAt
                };
            }).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> Profile(int id, string priority = "balanced")
        {
            var profile = await _context.RoommateProfiles.FindAsync(id);
            if (profile == null) return NotFound();

            var user = await _context.Users.FindAsync(profile.UserId);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            var viewModel = new RoommateProfileDetailViewModel
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.FullName ?? "Người dùng",
                AvatarUrl = user.AvatarUrl,
                Gender = profile.Gender ?? user.Gender,
                Occupation = user.Occupation,
                Bio = user.Bio,
                Budget = profile.Budget,
                PreferredArea = profile.PreferredArea,
                MoveInDate = profile.MoveInDate,
                Habits = profile.Habits?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
            };

            if (user.DateOfBirth.HasValue)
            {
                viewModel.Age = DateTime.Now.Year - user.DateOfBirth.Value.Year;
            }

            // Run AI Analysis if logged in and not viewing self
            if (currentUser != null && currentUser.Id != user.Id)
            {
                var analysis = await _aiMatchingService.AnalyzeCompatibilityAsync(currentUser.Id, user.Id, priority);
                viewModel.CompatibilityScore = analysis.OverallScore;
                viewModel.CompatibilityBreakdown = analysis.ComponentScores;
                viewModel.AiAnalysisReport = analysis.AnalysisReport;
                viewModel.SharedInterests = analysis.SharedInterests;
                viewModel.PotentialConflicts = analysis.PotentialConflicts;
            }
            else
            {
                // Default / Placeholder data for self-view or guest
                viewModel.CompatibilityScore = 0;
                viewModel.AiAnalysisReport = "Đăng nhập để xem phân tích mức độ phù hợp.";
            }

            // Check Connection Status
            if (currentUser != null)
            {
                if (currentUser.Id == user.Id)
                {
                    viewModel.ConnectionState = "self";
                }
                else
                {
                    var connection = await _context.Connections
                        .FirstOrDefaultAsync(c => (c.RequesterId == currentUser.Id && c.AddresseeId == user.Id) ||
                                                  (c.RequesterId == user.Id && c.AddresseeId == currentUser.Id));

                    if (connection != null)
                    {
                        viewModel.ConnectionId = connection.Id;
                        if (connection.Status == ConnectionStatus.Accepted)
                        {
                            viewModel.ConnectionState = "accepted";
                        }
                        else if (connection.Status == ConnectionStatus.Pending)
                        {
                            viewModel.ConnectionState = connection.RequesterId == currentUser.Id ? "pending_sent" : "pending_received";
                        }
                    }
                }
            }

            ViewBag.CurrentPriority = priority;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user already has a profile
            var existingProfile = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Pre-fill data if needed, or pass user data to View via ViewBag/ViewModel
            // For now, we assume a fresh form or we could pre-fill user details
            ViewBag.UserFullName = user.FullName;
            ViewBag.UserBirthYear = user.DateOfBirth?.Year;
            ViewBag.UserOccupation = user.Occupation;
            ViewBag.UserBio = user.Bio;
            ViewBag.UserAvatar = user.AvatarUrl;

            if (existingProfile != null)
            {
                return View(existingProfile);
            }

            return View(new RoommateProfile { Gender = user.Gender });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoommateProfile profile, string? FullName, int? BirthYear, string? Occupation, string? Introduction, IFormFile? avatarUpload)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Manually handle UserId validation as it's set in backend
            ModelState.Remove("UserId");

            // Set User linkage
            profile.UserId = user.Id;

            if (ModelState.IsValid)
            {
                bool userUpdated = false;

                // 1. Update User Profile Info
                if (!string.IsNullOrEmpty(FullName) && user.FullName != FullName)
                {
                    user.FullName = FullName;
                    userUpdated = true;
                }

                if (BirthYear.HasValue)
                {
                    // Update birth year while preserving month/day if possible, or default to 1/1
                    var currentDob = user.DateOfBirth ?? new DateTime(BirthYear.Value, 1, 1);
                    if (currentDob.Year != BirthYear.Value)
                    {
                        user.DateOfBirth = new DateTime(BirthYear.Value, currentDob.Month, currentDob.Day);
                        userUpdated = true;
                    }
                }

                if (!string.IsNullOrEmpty(Occupation) && user.Occupation != Occupation)
                {
                    user.Occupation = Occupation;
                    userUpdated = true;
                }

                if (!string.IsNullOrEmpty(Introduction) && user.Bio != Introduction)
                {
                    user.Bio = Introduction;
                    userUpdated = true;
                }

                if (!string.IsNullOrEmpty(profile.Gender) && user.Gender != profile.Gender)
                {
                    user.Gender = profile.Gender;
                    userUpdated = true;
                }

                // 2. Handle Avatar Upload
                if (avatarUpload != null && avatarUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarUpload.CopyToAsync(fileStream);
                    }

                    user.AvatarUrl = "/uploads/avatars/" + uniqueFileName;
                    userUpdated = true;
                }

                if (userUpdated)
                {
                    await _userManager.UpdateAsync(user);
                }

                // 3. Create or Update Roommate Profile
                var existingProfile = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (existingProfile != null)
                {
                    // Update
                    existingProfile.Budget = profile.Budget;
                    existingProfile.PreferredArea = profile.PreferredArea;
                    existingProfile.MoveInDate = profile.MoveInDate;
                    existingProfile.Habits = profile.Habits;
                    existingProfile.Gender = profile.Gender;
                    existingProfile.UpdatedAt = DateTime.Now;

                    _context.RoommateProfiles.Update(existingProfile);
                }
                else
                {
                    // Create
                    profile.CreatedAt = DateTime.Now;
                    profile.Status = "Active";
                    _context.RoommateProfiles.Add(profile);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If invalid, return view
            return View(profile);
        }

    
        public async Task<IActionResult> Match()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var currentProfile = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            // Get all other profiles
            var distinctUserIds = await _context.RoommateProfiles
                .Where(p => p.UserId != user.Id && p.Status == "Active")
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();

            var matches = new List<RoommateDisplayDto>();
            var users = await _context.Users
                .Where(u => distinctUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            var profiles = await _context.RoommateProfiles
                .Where(p => distinctUserIds.Contains(p.UserId))
                .ToListAsync();

            foreach (var profile in profiles)
            {
                users.TryGetValue(profile.UserId, out var otherUser);
                if (otherUser == null) continue;

                // Calculate Score
                var analysis = await _aiMatchingService.AnalyzeCompatibilityAsync(user.Id, profile.UserId);
                
                // Only show if score > 50 or top N
                if (analysis.OverallScore >= 50)
                {
                    int age = 0;
                    if (otherUser.DateOfBirth.HasValue)
                    {
                        age = DateTime.Now.Year - otherUser.DateOfBirth.Value.Year;
                    }

                    matches.Add(new RoommateDisplayDto
                    {
                        Id = profile.Id,
                        UserId = profile.UserId,
                        FullName = otherUser.FullName ?? "Người dùng",
                        AvatarUrl = otherUser.AvatarUrl,
                        Age = age,
                        Gender = profile.Gender ?? otherUser.Gender,
                        Occupation = otherUser.Occupation,
                        Budget = profile.Budget,
                        PreferredArea = profile.PreferredArea,
                        Habits = profile.Habits?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                        MatchPercentage = analysis.OverallScore,
                        CreatedAt = profile.CreatedAt
                    });
                }
            }

            // Sort by match percentage descending
            matches = matches.OrderByDescending(m => m.MatchPercentage).Take(10).ToList();

            return View(matches);
        }
    }
}
