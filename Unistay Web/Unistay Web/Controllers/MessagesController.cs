using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;

namespace Unistay_Web.Controllers
{
    [Authorize]
    [Route("api/messages")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            IWebHostEnvironment env,
            ILogger<MessagesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        // GET: api/messages/conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            // Get all friends
            var friends = await _context.Connections
                .Where(c => (c.RequesterId == currentUserId || c.AddresseeId == currentUserId) 
                    && c.Status == ConnectionStatus.Accepted)
                .Include(c => c.Requester)
                .Include(c => c.Addressee)
                .ToListAsync();

            var conversations = new List<object>();

            foreach (var connection in friends)
            {
                var friend = connection.RequesterId == currentUserId 
                    ? connection.Addressee 
                    : connection.Requester;

                if (friend == null) continue;

                // Get last message
                var lastMessage = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == friend.Id) ||
                               (m.SenderId == friend.Id && m.ReceiverId == currentUserId))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                // Count unread messages
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderId == friend.Id 
                        && m.ReceiverId == currentUserId 
                        && m.Status != MessageStatus.Seen);

                conversations.Add(new
                {
                    userId = friend.Id,
                    userName = friend.FullName ?? "Người dùng",
                    userAvatar = friend.AvatarUrl ?? "/images/default-avatar.png",
                    lastMessage = lastMessage != null ? new
                    {
                        content = lastMessage.IsDeleted ? "Tin nhắn đã bị xóa" : lastMessage.Content,
                        type = lastMessage.Type.ToString(),
                        createdAt = lastMessage.CreatedAt,
                        isSent = lastMessage.SenderId == currentUserId
                    } : null,
                    unreadCount = unreadCount,
                    isOnline = false // Will be updated by SignalR
                });
            }

            return Ok(conversations.OrderByDescending(c => ((dynamic)c).lastMessage?.createdAt ?? DateTime.MinValue));
        }

        // GET: api/messages/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMessages(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            // Check if blocked
            var isBlocked = await IsUserBlocked(currentUserId, userId);
            if (isBlocked)
                return BadRequest(new { message = "Bạn đã chặn hoặc bị chặn bởi người dùng này" });

            // Check friendship
            var areFriends = await _context.Connections
                .AnyAsync(c => ((c.RequesterId == currentUserId && c.AddresseeId == userId) ||
                               (c.RequesterId == userId && c.AddresseeId == currentUserId)) &&
                               c.Status == ConnectionStatus.Accepted);

            if (!areFriends)
                return BadRequest(new { message = "Bạn chỉ có thể nhắn tin với bạn bè" });

            var messagesQuery = _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId))
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .Include(m => m.Attachments)
                .OrderByDescending(m => m.CreatedAt);

            var totalMessages = await messagesQuery.CountAsync();
            var messages = await messagesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mark messages as delivered
            var undeliveredMessages = messages.Where(m => m.SenderId == userId && m.Status == MessageStatus.Sent);
            foreach (var msg in undeliveredMessages)
            {
                msg.Status = MessageStatus.Delivered;
                msg.DeliveredAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            var result = messages.Select(m => new
            {
                id = m.Id,
                content = m.IsDeleted ? "Tin nhắn đã bị xóa" : (m.IsEncrypted ? DecryptMessage(m.Content) : m.Content),
                type = m.Type.ToString(),
                status = m.Status.ToString(),
                senderId = m.SenderId,
                senderName = m.Sender?.FullName,
                senderAvatar = m.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                isSent = m.SenderId == currentUserId,
                isDeleted = m.IsDeleted,
                isEdited = m.IsEdited,
                editedAt = m.EditedAt,
                createdAt = m.CreatedAt,
                deliveredAt = m.DeliveredAt,
                seenAt = m.SeenAt,
                replyTo = m.ReplyToMessage != null ? new
                {
                    id = m.ReplyToMessage.Id,
                    content = m.ReplyToMessage.Content,
                    senderId = m.ReplyToMessage.SenderId
                } : null,
                attachments = m.Attachments?.Select(a => new
                {
                    id = a.Id,
                    fileName = a.FileName,
                    filePath = a.FilePath,
                    fileType = a.FileType,
                    fileSize = a.FileSize,
                    thumbnailPath = a.ThumbnailPath
                }).ToList()
            }).Reverse(); // Reverse to show oldest first

            return Ok(new
            {
                data = result,
                total = totalMessages,
                page = page,
                pageSize = pageSize
            });
        }

        // POST: api/messages/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (string.IsNullOrEmpty(model.ReceiverId) || string.IsNullOrEmpty(model.Content))
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            // Check if blocked
            var isBlocked = await IsUserBlocked(currentUserId, model.ReceiverId);
            if (isBlocked)
                return BadRequest(new { message = "Không thể gửi tin nhắn cho người dùng này" });

            // Check friendship
            var areFriends = await _context.Connections
                .AnyAsync(c => ((c.RequesterId == currentUserId && c.AddresseeId == model.ReceiverId) ||
                               (c.RequesterId == model.ReceiverId && c.AddresseeId == currentUserId)) &&
                               c.Status == ConnectionStatus.Accepted);

            if (!areFriends)
                return BadRequest(new { message = "Bạn chỉ có thể nhắn tin với bạn bè" });

            var message = new Message
            {
                SenderId = currentUserId,
                ReceiverId = model.ReceiverId,
                Content = model.IsEncrypted ? EncryptMessage(model.Content) : model.Content,
                Type = ParseMessageType(model.Type),
                Status = MessageStatus.Sent,
                IsEncrypted = model.IsEncrypted,
                ReplyToMessageId = model.ReplyToMessageId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Load sender info for response
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            return Ok(new
            {
                id = message.Id,
                content = model.Content,
                type = message.Type.ToString(),
                status = message.Status.ToString(),
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                senderName = message.Sender?.FullName,
                senderAvatar = message.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                createdAt = message.CreatedAt
            });
        }

        // POST: api/messages/{messageId}/mark-seen
        [HttpPost("{messageId}/mark-seen")]
        public async Task<IActionResult> MarkAsSeen(int messageId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return NotFound(new { message = "Tin nhắn không tồn tại" });

            if (message.ReceiverId != currentUserId)
                return Forbid();

            message.Status = MessageStatus.Seen;
            message.SeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu đã xem" });
        }

        // DELETE: api/messages/{messageId}
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return NotFound(new { message = "Tin nhắn không tồn tại" });

            if (message.SenderId != currentUserId)
                return Forbid();

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa tin nhắn" });
        }

        // POST: api/messages/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File không hợp lệ" });

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File quá lớn (tối đa 10MB)" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "messages");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/messages/{uniqueFileName}";

            return Ok(new
            {
                fileName = file.FileName,
                filePath = fileUrl,
                fileType = file.ContentType,
                fileSize = file.Length
            });
        }

        // POST: api/messages/block
        [HttpPost("block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserDto model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (string.IsNullOrEmpty(model.UserId))
                return BadRequest(new { message = "User ID không hợp lệ" });

            var existingBlock = await _context.BlockedUsers
                .FirstOrDefaultAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == model.UserId);

            if (existingBlock != null)
                return BadRequest(new { message = "Bạn đã chặn người dùng này" });

            var blockedUser = new BlockedUser
            {
                BlockerId = currentUserId,
                BlockedUserId = model.UserId,
                Reason = model.Reason,
                BlockedAt = DateTime.UtcNow
            };

            _context.BlockedUsers.Add(blockedUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã chặn người dùng" });
        }

        // DELETE: api/messages/unblock/{userId}
        [HttpDelete("unblock/{userId}")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var blockedUser = await _context.BlockedUsers
                .FirstOrDefaultAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == userId);

            if (blockedUser == null)
                return NotFound(new { message = "Không tìm thấy người dùng bị chặn" });

            _context.BlockedUsers.Remove(blockedUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã bỏ chặn người dùng" });
        }

        // POST: api/messages/report
        [HttpPost("report")]
        public async Task<IActionResult> ReportMessage([FromBody] ReportMessageDto model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var message = await _context.Messages.FindAsync(model.MessageId);
            if (message == null)
                return NotFound(new { message = "Tin nhắn không tồn tại" });

            var report = new MessageReport
            {
                ReporterId = currentUserId,
                MessageId = model.MessageId,
                Reason = model.Reason,
                Description = model.Description,
                Status = ReportStatus.Pending,
                ReportedAt = DateTime.UtcNow
            };

            _context.MessageReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã gửi báo cáo" });
        }

        // GET: api/messages/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchMessages([FromQuery] string query, [FromQuery] string? userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Từ khóa tìm kiếm không được để trống" });

            var messagesQuery = _context.Messages
                .Where(m => (m.SenderId == currentUserId || m.ReceiverId == currentUserId) 
                    && !m.IsDeleted
                    && m.Content != null && m.Content.Contains(query));

            if (!string.IsNullOrEmpty(userId))
            {
                messagesQuery = messagesQuery.Where(m => 
                    (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == currentUserId));
            }

            var messages = await messagesQuery
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .ToListAsync();

            var result = messages.Select(m => new
            {
                id = m.Id,
                content = m.Content,
                senderId = m.SenderId,
                receiverId = m.ReceiverId,
                senderName = m.Sender?.FullName,
                receiverName = m.Receiver?.FullName,
                createdAt = m.CreatedAt
            });

            return Ok(result);
        }

        // Helper methods
        private async Task<bool> IsUserBlocked(string userId1, string userId2)
        {
            return await _context.BlockedUsers
                .AnyAsync(b => (b.BlockerId == userId1 && b.BlockedUserId == userId2) ||
                              (b.BlockerId == userId2 && b.BlockedUserId == userId1));
        }

        private static MessageType ParseMessageType(string? type)
        {
            if (string.IsNullOrEmpty(type)) return MessageType.Text;
            if (Enum.TryParse<MessageType>(type, true, out var result)) return result;
            return MessageType.Text;
        }

        // Simple encryption/decryption (for demo purposes - use proper E2E encryption in production)
        private static string EncryptMessage(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            
            var data = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(data);
        }

        private static string DecryptMessage(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            
            try
            {
                var data = Convert.FromBase64String(cipherText);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return cipherText;
            }
        }
    }

    // DTOs
    public class SendMessageDto
    {
        public string? ReceiverId { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public bool IsEncrypted { get; set; } = false;
        public int? ReplyToMessageId { get; set; }
    }

    public class BlockUserDto
    {
        public string? UserId { get; set; }
        public string? Reason { get; set; }
    }

    public class ReportMessageDto
    {
        public int MessageId { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
    }
}
