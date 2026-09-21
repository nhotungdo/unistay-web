using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Roommate;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.DAL.Models.Connection;
using Microsoft.AspNetCore.Authorization;
using UnistayWeb.BLL.Services;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoommatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAiMatchingService _aiMatchingService;

        public RoommatesController(ApplicationDbContext context, UserManager<UserProfile> userManager, IWebHostEnvironment webHostEnvironment, IAiMatchingService aiMatchingService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _aiMatchingService = aiMatchingService;
        }

        // GET: api/Roommates
        [HttpGet]
        public async Task<IActionResult> GetRoommates([FromQuery] string? searchString, [FromQuery] string? gender, [FromQuery] string? budgetRange)
        {
            var query = _context.RoommateProfiles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.PreferredArea.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(gender) && gender != "All")
            {
                query = query.Where(p => p.Gender == gender);
            }

            if (!string.IsNullOrEmpty(budgetRange) && budgetRange != "All")
            {
                switch (budgetRange)
                {
                    case "low":
                        query = query.Where(p => p.Budget < 2000000);
                        break;
                    case "medium":
                        query = query.Where(p => p.Budget >= 2000000 && p.Budget <= 4000000);
                        break;
                    case "high":
                        query = query.Where(p => p.Budget > 4000000);
                        break;
                }
            }

            var profiles = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
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
                    age = DateTime.UtcNow.Year - user.DateOfBirth.Value.Year;
                }

                var displayGender = p.Gender ?? user?.Gender;

                return new RoommateDisplayDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    FullName = user?.FullName ?? "Người dùng",
                    AvatarUrl = user?.AvatarUrl,
                    Age = age,
                    Gender = displayGender,
                    Occupation = user?.Occupation,
                    Budget = p.Budget,
                    PreferredArea = p.PreferredArea,
                    Habits = p.Habits?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                    MatchPercentage = new Random(p.Id).Next(75, 99),
                    CreatedAt = p.CreatedAt
                };
            }).ToList();

            return Ok(viewModels);
        }

        // GET: api/Roommates/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id, [FromQuery] string priority = "balanced")
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
                viewModel.Age = DateTime.UtcNow.Year - user.DateOfBirth.Value.Year;
            }

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
                viewModel.CompatibilityScore = 0;
                viewModel.AiAnalysisReport = "Đăng nhập để xem phân tích mức độ phù hợp.";
            }

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

            return Ok(viewModel);
        }

        public class CreateRoommateDto
        {
            public decimal Budget { get; set; }
            public string? PreferredArea { get; set; }
            public DateTime? MoveInDate { get; set; }
            public string? Habits { get; set; }
            public string? Gender { get; set; }
            public string? FullName { get; set; }
            public int? BirthYear { get; set; }
            public string? Occupation { get; set; }
            public string? Introduction { get; set; }
            public IFormFile? AvatarUpload { get; set; }
        }

        // POST: api/Roommates
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> CreateProfile([FromForm] CreateRoommateDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool userUpdated = false;

            if (!string.IsNullOrEmpty(dto.FullName) && user.FullName != dto.FullName)
            {
                user.FullName = dto.FullName;
                userUpdated = true;
            }

            if (dto.BirthYear.HasValue)
            {
                var currentDob = user.DateOfBirth ?? new DateTime(dto.BirthYear.Value, 1, 1);
                if (currentDob.Year != dto.BirthYear.Value)
                {
                    user.DateOfBirth = new DateTime(dto.BirthYear.Value, currentDob.Month, currentDob.Day);
                    userUpdated = true;
                }
            }

            if (!string.IsNullOrEmpty(dto.Occupation) && user.Occupation != dto.Occupation)
            {
                user.Occupation = dto.Occupation;
                userUpdated = true;
            }

            if (!string.IsNullOrEmpty(dto.Introduction) && user.Bio != dto.Introduction)
            {
                user.Bio = dto.Introduction;
                userUpdated = true;
            }

            if (!string.IsNullOrEmpty(dto.Gender) && user.Gender != dto.Gender)
            {
                user.Gender = dto.Gender;
                userUpdated = true;
            }

            if (dto.AvatarUpload != null && dto.AvatarUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(dto.AvatarUpload.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AvatarUpload.CopyToAsync(fileStream);
                }

                user.AvatarUrl = "/uploads/avatars/" + uniqueFileName;
                userUpdated = true;
            }

            if (userUpdated)
            {
                await _userManager.UpdateAsync(user);
            }

            var existingProfile = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (existingProfile != null)
            {
                existingProfile.Budget = dto.Budget;
                existingProfile.PreferredArea = dto.PreferredArea;
                existingProfile.MoveInDate = dto.MoveInDate;
                existingProfile.Habits = dto.Habits;
                existingProfile.Gender = dto.Gender;
                existingProfile.UpdatedAt = DateTime.UtcNow;

                _context.RoommateProfiles.Update(existingProfile);
            }
            else
            {
                var profile = new RoommateProfile
                {
                    UserId = user.Id,
                    Budget = dto.Budget,
                    PreferredArea = dto.PreferredArea,
                    MoveInDate = dto.MoveInDate,
                    Habits = dto.Habits,
                    Gender = dto.Gender,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };
                _context.RoommateProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ tìm bạn ở ghép thành công!" });
        }

        // GET: api/Roommates/Match
        [HttpGet("Match")]
        [Authorize]
        public async Task<IActionResult> Match()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var currentProfile = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
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

                var analysis = await _aiMatchingService.AnalyzeCompatibilityAsync(user.Id, profile.UserId);
                
                if (analysis.OverallScore >= 50)
                {
                    int age = 0;
                    if (otherUser.DateOfBirth.HasValue)
                    {
                        age = DateTime.UtcNow.Year - otherUser.DateOfBirth.Value.Year;
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

            matches = matches.OrderByDescending(m => m.MatchPercentage).Take(10).ToList();

            return Ok(matches);
        }
    }
}
