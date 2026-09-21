using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Connection;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            IEmailService emailService,
            ILogger<ConnectionService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<PagedUserSearchResultDto> SearchUsersAsync(string? query, int page, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be empty");

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

            var updatedUsers = new List<UserSearchResultDto>();
            foreach (var u in userList)
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

                updatedUsers.Add(new UserSearchResultDto
                {
                    Id = u.Id,
                    Name = u.FullName ?? "Người dùng",
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl ?? "/images/default-avatar.png",
                    ConnectionStatus = status
                });
            }

            return new PagedUserSearchResultDto
            {
                Data = updatedUsers,
                Total = totalItems,
                Page = page
            };
        }

        public async Task<ServiceResult> SendRequestAsync(string targetUserId, string currentUserId)
        {
            var targetUser = await _context.Users.FindAsync(targetUserId);
            if (targetUser == null)
                return new ServiceResult { Success = false, Message = "User not found.", HttpStatusCode = 404 };

            var existing = await _context.Connections
                .FirstOrDefaultAsync(c => (c.RequesterId == currentUserId && c.AddresseeId == targetUserId) ||
                                          (c.RequesterId == targetUserId && c.AddresseeId == currentUserId));

            if (existing != null)
            {
                if (existing.Status == ConnectionStatus.Pending)
                    return new ServiceResult { Success = false, Message = "Connection request already pending." };
                if (existing.Status == ConnectionStatus.Accepted)
                    return new ServiceResult { Success = false, Message = "Already connected." };

                if (existing.Status == ConnectionStatus.Declined)
                {
                    existing.Status = ConnectionStatus.Pending;
                    existing.RequesterId = currentUserId;
                    existing.AddresseeId = targetUserId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return new ServiceResult { Success = true, Message = "Request sent successfully." };
                }
            }

            var connection = new Connection
            {
                RequesterId = currentUserId,
                AddresseeId = targetUserId,
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send connection request email to {Email}", targetUser.Email);
            }

            return new ServiceResult { Success = true, Message = "Request sent successfully." };
        }

        public async Task<ServiceResult> RespondRequestAsync(int connectionId, string action, string currentUserId)
        {
            var connection = await _context.Connections
                .FirstOrDefaultAsync(c => c.Id == connectionId && c.AddresseeId == currentUserId);

            if (connection == null)
                return new ServiceResult { Success = false, Message = "Connection request not found or you are not authorized.", HttpStatusCode = 404 };

            if (connection.Status != ConnectionStatus.Pending)
                return new ServiceResult { Success = false, Message = "Request is already " + connection.Status };

            if (action.ToLower() == "accept")
            {
                connection.Status = ConnectionStatus.Accepted;
            }
            else if (action.ToLower() == "reject")
            {
                connection.Status = ConnectionStatus.Declined;
            }
            else
            {
                return new ServiceResult { Success = false, Message = "Invalid action. Use 'accept' or 'reject'." };
            }

            connection.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Connection " + (action == "accept" ? "accepted" : "rejected") };
        }

        public async Task<List<PendingRequestDto>> GetPendingRequestsAsync(string currentUserId)
        {
            return await _context.Connections
                .Where(c => c.AddresseeId == currentUserId && c.Status == ConnectionStatus.Pending)
                .Include(c => c.Requester)
                .Select(c => new PendingRequestDto
                {
                    ConnectionId = c.Id,
                    RequesterName = c.Requester.FullName,
                    RequesterEmail = c.Requester.Email,
                    RequesterAvatar = c.Requester.AvatarUrl ?? "/images/default-avatar.png",
                    SentAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string currentUserId)
        {
            var friends = await _context.Connections
                .AsNoTracking()
                .Where(c => (c.RequesterId == currentUserId || c.AddresseeId == currentUserId) && c.Status == ConnectionStatus.Accepted)
                .Include(c => c.Requester)
                .Include(c => c.Addressee)
                .ToListAsync();

            var result = new List<FriendDto>();

            foreach (var c in friends)
            {
                var friend = c.RequesterId == currentUserId ? c.Addressee : c.Requester;
                if (friend == null) continue;

                result.Add(new FriendDto
                {
                    FriendId = friend.Id,
                    FriendName = friend.FullName ?? "Người dùng",
                    FriendAvatar = friend.AvatarUrl ?? "/images/default-avatar.png",
                    ConnectedSince = c.UpdatedAt ?? c.CreatedAt
                });
            }

            return result;
        }

        public async Task<List<ConnectionSuggestionDto>> GetSuggestionsAsync(string currentUserId)
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null)
                return new List<ConnectionSuggestionDto>();

            var excludedIds = await _context.Connections
                .Where(c => c.RequesterId == currentUserId || c.AddresseeId == currentUserId)
                .Select(c => c.RequesterId == currentUserId ? c.AddresseeId : c.RequesterId)
                .ToListAsync();

            excludedIds.Add(currentUserId);

            var candidates = await _context.Users
                .Where(u => !excludedIds.Contains(u.Id) && u.IsActive && !u.IsBlocked)
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.AvatarUrl,
                    u.City,
                    u.District,
                    u.Occupation,
                    u.Bio,
                    u.CreatedAt
                })
                .Take(100)
                .ToListAsync();

            var scoredCandidates = candidates.Select(c =>
            {
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
            .Select(x => new ConnectionSuggestionDto
            {
                Id = x.Candidate.Id,
                Name = x.Candidate.FullName ?? "Người dùng",
                AvatarUrl = x.Candidate.AvatarUrl ?? "/images/default-avatar.png",
                MutualInfo = GetMutualInfo(currentUser, x.Candidate.City, x.Candidate.Occupation),
                Score = x.Score
            })
            .ToList();

            return scoredCandidates;
        }

        public async Task<List<string>> GetUserFriendIdsAsync(string userId)
        {
            var friends = await _context.Connections
                .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
                .Select(c => c.RequesterId == userId ? c.AddresseeId : c.RequesterId)
                .ToListAsync();

            return friends.Where(f => f != null).ToList()!;
        }

        public async Task<List<ConnectionUserDto>> GetUserFriendsAsync(string userId)
        {
            var friendIds = await GetUserFriendIdsAsync(userId);

            if (!friendIds.Any())
                return new List<ConnectionUserDto>();

            return await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .Select(u => new ConnectionUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "Người dùng",
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();
        }

        public async Task<List<ChatGroup>> GetUserGroupsAsync(string userId)
        {
            var groupIds = await _context.ChatGroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.ChatGroupId)
                .ToListAsync();

            if (!groupIds.Any())
                return new List<ChatGroup>();

            return await _context.ChatGroups
                .Where(g => groupIds.Contains(g.Id))
                .ToListAsync();
        }

        private static string GetMutualInfo(UserProfile? user, string? city, string? occupation)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(user?.City) && string.Equals(user.City, city, StringComparison.OrdinalIgnoreCase))
                parts.Add($"Sống tại {city}");
            if (!string.IsNullOrEmpty(user?.Occupation) && !string.IsNullOrEmpty(occupation) && user.Occupation.Contains(occupation, StringComparison.OrdinalIgnoreCase))
                parts.Add("Cùng nghề nghiệp");

            if (parts.Count == 0) return "Gợi ý cho bạn";
            return string.Join(" • ", parts);
        }
    }
}
