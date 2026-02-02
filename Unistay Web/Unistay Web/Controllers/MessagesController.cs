using Microsoft.AspNetCore.Authorization;
using Unistay_Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Unistay_Web.Hubs;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;

namespace Unistay_Web.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [Route("api/messages")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MessagesController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            IWebHostEnvironment env,
            ILogger<MessagesController> logger,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
            _hubContext = hubContext;
        }

        // GET: api/messages/conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId == null)
                {
                    _logger.LogWarning("GetConversations called with no authenticated user");
                    return Unauthorized(new { message = "Bạn cần đăng nhập để xem tin nhắn" });
                }

                _logger.LogInformation("Loading conversations for user {UserId}", currentUserId);

                // 1. Get recent DIRECT conversations
                var directStats = new List<(string? UserId, Message? LastMessage, int UnreadCount)>();
                try
                {
                    var tempDirectStats = await _context.Messages
                        .Where(m => !m.IsDeleted && m.ChatGroupId == null && (m.SenderId == currentUserId || m.ReceiverId == currentUserId))
                        .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                        .Select(g => new
                        {
                            UserId = g.Key,
                            LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                            UnreadCount = g.Count(m => m.ReceiverId == currentUserId && m.Status != MessageStatus.Seen)
                        })
                        .ToListAsync();
                    
                    directStats = tempDirectStats.Select(x => (x.UserId, x.LastMessage, x.UnreadCount)).ToList();
                    _logger.LogInformation("Found {Count} direct conversations", directStats.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading direct conversations for user {UserId}. Messages table may not exist or be accessible.", currentUserId);
                    // Continue with empty list - don't fail the entire request
                }

                // 2. Get GROUP conversations (with error handling for missing tables)
                List<int> userGroupIds = new List<int>();
                var groupStats = new List<(int? GroupId, Message? LastMessage, int UnreadCount)>();
                Dictionary<int, ChatGroup> groups = new Dictionary<int, ChatGroup>();

                try
                {
                    userGroupIds = await _context.ChatGroupMembers
                        .Where(m => m.UserId == currentUserId)
                        .Select(m => m.ChatGroupId)
                        .ToListAsync();

                    _logger.LogInformation("User is member of {Count} groups", userGroupIds.Count);

                    if (userGroupIds.Any())
                    {
                        var tempGroupStats = await _context.Messages
                            .Where(m => !m.IsDeleted && m.ChatGroupId != null && userGroupIds.Contains(m.ChatGroupId.Value))
                            .GroupBy(m => m.ChatGroupId)
                            .Select(g => new
                            {
                                GroupId = g.Key,
                                LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                                UnreadCount = g.Count(m => m.SenderId != currentUserId && m.Status != MessageStatus.Seen)
                            })
                            .ToListAsync();

                        groupStats = tempGroupStats.Select(x => (x.GroupId, x.LastMessage, x.UnreadCount)).ToList();

                        groups = await _context.ChatGroups
                            .Where(g => userGroupIds.Contains(g.Id))
                            .ToDictionaryAsync(g => g.Id);
                        
                        _logger.LogInformation("Loaded {Count} group conversations", groupStats.Count);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue - group chat feature might not be set up yet
                    _logger.LogWarning(ex, "Error loading group conversations for user {UserId}. Group chat tables may not exist yet.", currentUserId);
                }

                // 3. Get User & Group Details
                var directUserIds = directStats
                    .Select(c => c.UserId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                // Friends (always show) - with error handling
                List<string?> friendIds = new List<string?>();
                try
                {
                    friendIds = await _context.Connections
                        .Where(c => (c.RequesterId == currentUserId || c.AddresseeId == currentUserId) && c.Status == ConnectionStatus.Accepted)
                        .Select(c => c.RequesterId == currentUserId ? c.AddresseeId : c.RequesterId)
                        .ToListAsync();
                    
                    _logger.LogInformation("Found {Count} friends", friendIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading friends for user {UserId}. Connections table may not exist.", currentUserId);
                    // Continue with empty list
                }

                var allUserIds = directUserIds
                    .Union(friendIds)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .Cast<string>()
                    .ToList();

                var users = new Dictionary<string, (string FullName, string AvatarUrl)>();
                if (allUserIds.Any())
                {
                    try
                    {
                        users = await _context.Users
                            .Where(u => allUserIds.Contains(u.Id))
                            .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
                            .ToDictionaryAsync(
                                u => u.Id, 
                                u => (u.FullName ?? "Người dùng", u.AvatarUrl ?? "/images/default-avatar.png")
                            );
                        
                        _logger.LogInformation("Loaded {Count} user profiles", users.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading user profiles. Users table may be missing columns.");
                        // Continue with empty dictionary
                    }
                }

                var result = new List<ConversationViewModel>();

                // Process Direct
                foreach (var stat in directStats)
                {
                    try
                    {
                        string? userId = stat.UserId;
                        if (string.IsNullOrEmpty(userId) || !users.ContainsKey(userId)) continue;
                        
                        var user = users[userId];
                        Message? lm = stat.LastMessage;

                        result.Add(new ConversationViewModel
                        {
                            UserId = userId,
                            IsGroup = false,
                            UserName = user.FullName,
                            UserAvatar = user.AvatarUrl,
                            LastMessage = lm != null ? new MessageViewModel
                            {
                                Content = lm.IsDeleted ? "Tin nhắn đã bị xóa" : (lm.IsEncrypted ? DecryptMessage(lm.Content) : lm.Content),
                                Type = lm.Type.ToString(),
                                CreatedAt = lm.CreatedAt,
                                IsSent = lm.SenderId == currentUserId
                            } : null,
                            UnreadCount = stat.UnreadCount,
                            IsOnline = false
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing direct conversation");
                        continue;
                    }
                }

                // Process Groups (only if we have group data)
                foreach (var stat in groupStats)
                {
                    try
                    {
                        if (stat.GroupId == null || !groups.ContainsKey(stat.GroupId.Value)) continue;
                        var grp = groups[stat.GroupId.Value];
                        var lm = stat.LastMessage;

                        result.Add(new ConversationViewModel
                        {
                            GroupId = grp.Id,
                            IsGroup = true,
                            UserName = grp.Name ?? "Nhóm chat",
                            UserAvatar = "/images/group-default.png",
                            LastMessage = lm != null ? new MessageViewModel
                            {
                                Content = lm.IsDeleted ? "T/N đã xóa" : (lm.SenderId == currentUserId ? "Bạn: " : "") + (lm.IsEncrypted ? DecryptMessage(lm.Content) : lm.Content),
                                Type = lm.Type.ToString(),
                                CreatedAt = lm.CreatedAt,
                                IsSent = lm.SenderId == currentUserId
                            } : null,
                            UnreadCount = stat.UnreadCount
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing group conversation");
                        continue;
                    }
                }

                // Add empty friends
                foreach (var friendId in friendIds)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(friendId) || result.Any(r => !r.IsGroup && r.UserId == friendId) || !users.ContainsKey(friendId)) continue;
                        var user = users[friendId];
                        result.Add(new ConversationViewModel
                        {
                            UserId = friendId,
                            IsGroup = false,
                            UserName = user.FullName,
                            UserAvatar = user.AvatarUrl,
                            LastMessage = null,
                            UnreadCount = 0,
                            IsOnline = false
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error adding friend to conversations");
                        continue;
                    }
                }

                // Add empty groups (only if we have group data)
                foreach (var gid in userGroupIds)
                {
                    try
                    {
                        if (result.Any(r => r.IsGroup && r.GroupId == gid) || !groups.ContainsKey(gid)) continue;
                        var grp = groups[gid];
                        result.Add(new ConversationViewModel
                        {
                            GroupId = grp.Id,
                            IsGroup = true,
                            UserName = grp.Name ?? "Nhóm chat",
                            UserAvatar = "/images/group-default.png",
                            LastMessage = null,
                            UnreadCount = 0
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error adding group to conversations");
                        continue;
                    }
                }

                _logger.LogInformation("Returning {Count} total conversations for user {UserId}", result.Count, currentUserId);
                return Ok(result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in GetConversations");
                return StatusCode(500, new { message = "Lỗi khi tải danh sách cuộc trò chuyện", error = ex.Message, details = ex.StackTrace });
            }
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

            // Mark delivered asynchronously (fire and forget or separate task if highly loaded)
            // For now, simple update is fine.
            var unreadMsgs = messages.Where(m => m.SenderId == userId && m.Status == MessageStatus.Sent).ToList();
            if (unreadMsgs.Any())
            {
                foreach (var msg in unreadMsgs)
                {
                    msg.Status = MessageStatus.Delivered;
                    msg.DeliveredAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

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
            }).Reverse();

            return Ok(new
            {
                data = result,
                total = totalMessages,
                page = page,
                pageSize = pageSize
            });
        }

        // POST: api/messages/send
        // GET: api/messages/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupMessages(int groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var isMember = await _context.ChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == currentUserId);
            if (!isMember) return Forbid();

            var messagesQuery = _context.Messages
                .Where(m => m.ChatGroupId == groupId && !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.ReplyToMessage)
                .Include(m => m.Attachments)
                .OrderByDescending(m => m.CreatedAt);

            var totalMessages = await messagesQuery.CountAsync();
            var messages = await messagesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
            }).Reverse();

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

            if (string.IsNullOrEmpty(model.Content) && (model.AttachmentPaths == null || !model.AttachmentPaths.Any()))
                return BadRequest(new { message = "Nội dung tin nhắn không được để trống" });

            if (string.IsNullOrEmpty(model.ReceiverId) && model.GroupId == null)
                return BadRequest(new { message = "Phải chỉ định người nhận hoặc nhóm" });

            // Check blocked if direct
            if (!string.IsNullOrEmpty(model.ReceiverId))
            {
                var isBlocked = await IsUserBlocked(currentUserId, model.ReceiverId);
                if (isBlocked) return BadRequest(new { message = "Không thể gửi tin nhắn cho người dùng này" });
            }

            // Check group membership if group
            if (model.GroupId.HasValue)
            {
                var isMember = await _context.ChatGroupMembers.AnyAsync(m => m.ChatGroupId == model.GroupId && m.UserId == currentUserId);
                if (!isMember) return Forbid();
            }

            var message = new Message
            {
                SenderId = currentUserId,
                ReceiverId = model.ReceiverId,
                ChatGroupId = model.GroupId,
                Content = model.IsEncrypted ? EncryptMessage(model.Content) : model.Content,
                Type = ParseMessageType(model.Type),
                Status = MessageStatus.Sent,
                IsEncrypted = model.IsEncrypted,
                ReplyToMessageId = model.ReplyToMessageId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Handle Attachments
            if (model.AttachmentPaths != null && model.AttachmentPaths.Any())
            {
                message.Attachments = new List<MessageAttachment>();
                foreach (var path in model.AttachmentPaths)
                {
                    message.Attachments.Add(new MessageAttachment
                    {
                        FilePath = path,
                        FileName = Path.GetFileName(path),
                        FileType = "file",
                        FileSize = 0
                    });

                    if (message.Type == MessageType.Text) message.Type = MessageType.File;
                }
            }

            await _context.SaveChangesAsync();
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            var messageData = new
            {
                id = message.Id,
                content = model.Content,
                type = message.Type.ToString(),
                status = message.Status.ToString(),
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                groupId = message.ChatGroupId,
                senderName = message.Sender?.FullName,
                senderAvatar = message.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                createdAt = message.CreatedAt,
                attachmentPaths = model.AttachmentPaths
            };

            // Broadcast via SignalR
            if (model.GroupId.HasValue)
            {
                await _hubContext.Clients.Group($"group_{model.GroupId}").SendAsync("ReceiveMessage", messageData);
            }
            else if (!string.IsNullOrEmpty(model.ReceiverId))
            {
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("ReceiveMessage", messageData);
                // Also send to Sender (to show in their UI if they have multiple tabs/devices)
                // Note: The caller's UI will also add it manually usually, but this ensures sync.
                // However, our JS currently waits for response and adds it? Or listens to SignalR?
                // JS: connection.on("ReceiveMessage") checks receiverId or senderId.
                // If we broadcast to Sender, we get double?
                // JS Logic: if (message.receiverId === currentUserId || message.senderId === currentChatUserId)
                // When I send: receiverId = target. senderId = ME.
                // currentUserId = ME. currentChatUserId = target.
                // So (target === ME) False. (ME === target) False.
                // Sender logic: ME sending to Target.
                // We need to notify ME's other devices.
                await _hubContext.Clients.Group(currentUserId).SendAsync("ReceiveMessage", messageData);
            }

            return Ok(messageData);
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
        private static string EncryptMessage(string? plainText) => MessageEncryptionHelper.Encrypt(plainText);
        private static string DecryptMessage(string? cipherText) => MessageEncryptionHelper.Decrypt(cipherText);
    }

}
