using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;
using Unistay_Web.Services;

namespace Unistay_Web.Controllers
{
    [Authorize]
    public class ConnectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IEmailService _emailService;

        public ConnectionsController(ApplicationDbContext context, UserManager<UserProfile> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Messages");
        }

        [HttpGet("api/users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? query, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query cannot be empty" });

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();
            var pageSize = 10;

            var usersQuery = _context.Users
                .Where(u => u.Id != currentUserId && 
                           (u.Email.Contains(query) || u.FullName.Contains(query)))
                .AsNoTracking();

            var totalItems = await usersQuery.CountAsync();
            
            var userList = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.Id, u.FullName, u.Email, u.AvatarUrl })
                .ToListAsync();

             var updatedUsers = new List<object>();
             foreach(var u in userList)
             {
                 var conn = await _context.Connections
                     .FirstOrDefaultAsync(c => (c.RequesterId == currentUserId && c.AddresseeId == u.Id) || 
                                               (c.RequesterId == u.Id && c.AddresseeId == currentUserId));
                 
                 string status = "none";
                 if (conn != null)
                 {
                     if (conn.Status == ConnectionStatus.Accepted) status = "accepted";
                     else if (conn.Status == ConnectionStatus.Pending)
                     {
                         status = conn.RequesterId == currentUserId ? "pending_sent" : "pending_received";
                     }
                     else if (conn.Status == ConnectionStatus.Declined) status = "declined";
                 }

                 updatedUsers.Add(new {
                     id = u.Id,
                     name = u.FullName,
                     email = u.Email,
                     avatarUrl = u.AvatarUrl ?? "/images/default-avatar.png",
                     connectionStatus = status
                 });
             }
             
            return Ok(new { data = updatedUsers, total = totalItems, page });
        }

        [HttpPost("api/connections/request")]
        public async Task<IActionResult> SendRequest([FromBody] ConnectionRequestDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.TargetUserId))
                return BadRequest(new { message = "Invalid data." });

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (currentUserId == model.TargetUserId)
                return BadRequest(new { message = "Cannot send request to yourself." });

            var targetUser = await _context.Users.FindAsync(model.TargetUserId);
            if (targetUser == null)
                return NotFound(new { message = "User not found." });

            var existing = await _context.Connections
                .FirstOrDefaultAsync(c => (c.RequesterId == currentUserId && c.AddresseeId == model.TargetUserId) ||
                                          (c.RequesterId == model.TargetUserId && c.AddresseeId == currentUserId));

            if (existing != null)
            {
                if (existing.Status == ConnectionStatus.Pending)
                    return BadRequest(new { message = "Connection request already pending." });
                if (existing.Status == ConnectionStatus.Accepted)
                    return BadRequest(new { message = "Already connected." });
                
                // If previously declined, we might allow resending, but keeping it simple for now or resetting it.
                // Let's allow resending if declined, by updating the existing record.
                if (existing.Status == ConnectionStatus.Declined)
                {
                    existing.Status = ConnectionStatus.Pending;
                    existing.RequesterId = currentUserId; // New requester is current user
                    existing.AddresseeId = model.TargetUserId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                     return Ok(new { message = "Request sent successfully." });
                }
            }

            var connection = new Connection
            {
                RequesterId = currentUserId,
                AddresseeId = model.TargetUserId,
                Status = ConnectionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Connections.Add(connection);
            await _context.SaveChangesAsync();

            try 
            {
                await _emailService.SendEmailAsync(targetUser.Email!, "New Connection Request", 
                    $"Hello {targetUser.FullName},<br>You have received a new connection request on Unistay.");
            }
            catch {} // Don't fail the request if email fails

            return Ok(new { message = "Request sent successfully." });
        }

        [HttpPost("api/connections/respond")]
        public async Task<IActionResult> RespondRequest([FromBody] RespondRequestDto model)
        {
             if (model == null) return BadRequest(new { message = "Invalid data." });
             
             if (model.Action == null) return BadRequest(new { message = "Action is required." });

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();
            
            var connection = await _context.Connections
                .FirstOrDefaultAsync(c => c.Id == model.ConnectionId && c.AddresseeId == currentUserId);

            if (connection == null)
                return NotFound(new { message = "Connection request not found or you are not authorized." });

            if (connection.Status != ConnectionStatus.Pending)
                return BadRequest(new { message = "Request is already " + connection.Status });

            if (model.Action.ToLower() == "accept")
            {
                connection.Status = ConnectionStatus.Accepted;
            }
            else if (model.Action.ToLower() == "reject")
            {
                connection.Status = ConnectionStatus.Declined;
            }
            else
            {
                return BadRequest(new { message = "Invalid action. Use 'accept' or 'reject'." });
            }
            
            connection.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Connection " + (model.Action == "accept" ? "accepted" : "rejected") });
        }

        [HttpGet("api/connections/pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();
            var requests = await _context.Connections
                .Where(c => c.AddresseeId == currentUserId && c.Status == ConnectionStatus.Pending)
                .Include(c => c.Requester)
                .Select(c => new {
                    connectionId = c.Id,
                    requesterName = c.Requester.FullName,
                    requesterEmail = c.Requester.Email,
                    requesterAvatar = c.Requester.AvatarUrl ?? "/images/default-avatar.png",
                    sentAt = c.CreatedAt
                })
                .ToListAsync();
            
            return Ok(requests);
        }

        [HttpGet("api/connections/friends")]
        public async Task<IActionResult> GetFriends()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var friends = await _context.Connections
                .Where(c => (c.RequesterId == currentUserId || c.AddresseeId == currentUserId) && c.Status == ConnectionStatus.Accepted)
                .Include(c => c.Requester)
                .Include(c => c.Addressee)
                .Select(c => new
                {
                    friendId = c.RequesterId == currentUserId ? c.AddresseeId : c.RequesterId,
                    friendName = c.RequesterId == currentUserId ? c.Addressee.FullName : c.Requester.FullName,
                    friendAvatar = (c.RequesterId == currentUserId ? c.Addressee.AvatarUrl : c.Requester.AvatarUrl) ?? "/images/default-avatar.png",
                    connectedSince = c.UpdatedAt ?? c.CreatedAt
                })
                .ToListAsync();

            return Ok(friends);
        }

        [HttpGet("api/connections/suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null) return NotFound();

            // 1. Get IDs to exclude (Self, Friends, Pending, Blocked)
            var excludedIds = await _context.Connections
                .Where(c => c.RequesterId == currentUserId || c.AddresseeId == currentUserId)
                .Select(c => c.RequesterId == currentUserId ? c.AddresseeId : c.RequesterId)
                .ToListAsync();
            
            excludedIds.Add(currentUserId);

            // 2. Query potential candidates
            var candidates = await _context.Users
                .Where(u => !excludedIds.Contains(u.Id) && u.IsActive && !u.IsBlocked)
                .Select(u => new { 
                    u.Id, 
                    u.FullName, 
                    u.Email, 
                    u.AvatarUrl,
                    u.City,
                    u.District,
                    u.Occupation,
                    u.Bio,
                    u.CreatedAt
                })
                .Take(100)
                .ToListAsync();

            // 3. Score candidates
            var scoredCandidates = candidates.Select(c => {
                int score = 0;
                
                if (!string.IsNullOrEmpty(currentUser.City) && string.Equals(c.City, currentUser.City, StringComparison.OrdinalIgnoreCase))
                    score += 3;
                if (!string.IsNullOrEmpty(currentUser.District) && string.Equals(c.District, currentUser.District, StringComparison.OrdinalIgnoreCase))
                    score += 2;
                
                if (!string.IsNullOrEmpty(currentUser.Occupation) && !string.IsNullOrEmpty(c.Occupation) && 
                    currentUser.Occupation.Contains(c.Occupation, StringComparison.OrdinalIgnoreCase))
                    score += 2;

                return new { Candidate = c, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Candidate.CreatedAt)
            .Take(6)
            .Select(x => new {
                id = x.Candidate.Id,
                name = x.Candidate.FullName,
                avatarUrl = x.Candidate.AvatarUrl ?? "/images/default-avatar.png",
                mutualInfo = GetMutualInfo(currentUser, x.Candidate.City, x.Candidate.Occupation),
                score = x.Score
            })
            .ToList();

            return Ok(scoredCandidates);
        }

        private string GetMutualInfo(UserProfile user, string? city, string? occupation)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(user.City) && string.Equals(user.City, city, StringComparison.OrdinalIgnoreCase))
                parts.Add($"Sống tại {city}");
            if (!string.IsNullOrEmpty(user.Occupation) && !string.IsNullOrEmpty(occupation) && user.Occupation.Contains(occupation, StringComparison.OrdinalIgnoreCase))
                parts.Add("Cùng nghề nghiệp");
            
            if (parts.Count == 0) return "Gợi ý cho bạn";
            return string.Join(" • ", parts);
        }
    }
    
    public class ConnectionRequestDto
    {
        public string? TargetUserId { get; set; }
    }

    public class RespondRequestDto
    {
        public int ConnectionId { get; set; }
        public string? Action { get; set; }
    }
}
