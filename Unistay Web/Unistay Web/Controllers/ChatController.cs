using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.User;
using Unistay_Web.Models.Chat;

namespace Unistay_Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly Unistay_Web.Services.IFileUploadService _fileUploadService;

        public ChatController(ApplicationDbContext context, UserManager<UserProfile> userManager, Unistay_Web.Services.IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Messages");
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var userId = _userManager.GetUserId(User);
            
            var conversationsData = await _context.UserConversations
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Conversation)
                    .ThenInclude(c => c.Messages)
                .Include(uc => uc.Conversation)
                    .ThenInclude(c => c.UserConversations)
                        .ThenInclude(uc2 => uc2.User)
                .Select(uc => new {
                    conversation = uc.Conversation,
                    otherUser = uc.Conversation.UserConversations.FirstOrDefault(u => u.UserId != userId).User,
                    lastMessage = uc.Conversation.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
                })
                .ToListAsync();

            var result = conversationsData
                .OrderByDescending(x => x.lastMessage?.CreatedAt ?? DateTime.MinValue)
                .Select(x => new {
                    id = x.conversation.Id,
                    name = x.otherUser?.FullName ?? "Unknown User",
                    avatar = x.otherUser?.AvatarUrl ?? "/images/default-avatar.png",
                    lastMessage = x.lastMessage?.Content ?? "Start a conversation",
                    time = FormatTime(x.lastMessage?.CreatedAt),
                    unread = x.conversation.Messages.Count(m => !m.IsRead && m.ReceiverId == userId),
                    otherUserId = x.otherUser?.Id
                });

            return Json(result);
        }

        private static string FormatTime(DateTime? time)
        {
            if (!time.HasValue) return "";
            var now = DateTime.UtcNow;
            var diff = now - time.Value;
            if (diff.TotalDays < 1) return time.Value.ToLocalTime().ToString("HH:mm");
            if (diff.TotalDays < 7) return time.Value.ToLocalTime().ToString("ddd");
            return time.Value.ToLocalTime().ToString("dd/MM/yy");
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(int conversationId)
        {
             var userId = _userManager.GetUserId(User);
             var isParticipant = await _context.UserConversations
                 .AnyAsync(uc => uc.UserId == userId && uc.ConversationId == conversationId);
                 
             if (!isParticipant) return Forbid();

             var messages = await _context.Messages
                 .Where(m => m.ConversationId == conversationId)
                 .OrderBy(m => m.CreatedAt)
                 .Include(m => m.Sender)
                 .Select(m => new {
                     id = m.Id,
                     senderId = m.SenderId,
                     senderName = m.Sender.FullName,
                     senderAvatar = m.Sender.AvatarUrl ?? "/images/default-avatar.png",
                     content = m.Content,
                     createdAt = m.CreatedAt, // Client side formatting
                     isMe = m.SenderId == userId,
                     attachmentUrl = m.AttachmentUrl,
                     type = m.Type,
                     isRead = m.IsRead
                 })
                 .ToListAsync();
                 
             return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Json(new List<object>());
            var userId = _userManager.GetUserId(User);
            
            var users = await _context.Users
                .Where(u => u.Id != userId && (u.FullName.Contains(query) || u.Email.Contains(query)))
                .Take(10)
                .Select(u => new {
                    id = u.Id,
                    name = u.FullName,
                    avatar = u.AvatarUrl ?? "/images/default-avatar.png",
                    city = u.City
                })
                .ToListAsync();

            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            try
            {
                var url = await _fileUploadService.UploadImageAsync(file, "chat-attachments");
                return Json(new { success = true, url, type = file.ContentType.StartsWith("image/") ? "Image" : "File" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetConversationId(string targetUserId)
        {
             var currentUserId = _userManager.GetUserId(User);
             var existing = await _context.UserConversations
                 .Where(uc => uc.UserId == currentUserId)
                 .Select(uc => uc.Conversation)
                 .Where(c => c.UserConversations.Any(uc => uc.UserId == targetUserId))
                 .Select(c => c.Id)
                 .FirstOrDefaultAsync();
             
             return Json(new { conversationId = existing });
        }
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
             var user = await _context.Users.FindAsync(userId);
             if(user == null) return NotFound();
             return Json(new { 
                 id = user.Id, 
                 name = user.FullName, 
                 avatar = user.AvatarUrl ?? "/images/default-avatar.png" 
             });
        }
    }
}
