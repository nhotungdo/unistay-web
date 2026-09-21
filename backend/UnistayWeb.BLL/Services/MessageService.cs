using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Connection;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            ApplicationDbContext context,
            UserManager<UserProfile> userManager,
            IWebHostEnvironment env,
            ILogger<MessageService> logger)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        public async Task<List<ConversationViewModel>> GetConversationsAsync(string currentUserId)
        {
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
            }

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
                _logger.LogWarning(ex, "Error loading group conversations for user {UserId}. Group chat tables may not exist yet.", currentUserId);
            }

            var directUserIds = directStats
                .Select(c => c.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

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
                }
            }

            var result = new List<ConversationViewModel>();

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
            return result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue).ToList();
        }

        public async Task<PagedResultDto<MessageResultDto>> GetMessagesAsync(string currentUserId, string userId, int page, int pageSize)
        {
            var isBlocked = await IsUserBlockedAsync(currentUserId, userId);
            if (isBlocked)
                throw new InvalidOperationException("Bạn đã chặn hoặc bị chặn bởi người dùng này");

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

            var result = messages.Select(m => new MessageResultDto
            {
                Id = m.Id,
                Content = m.IsDeleted ? "Tin nhắn đã bị xóa" : (m.IsEncrypted ? DecryptMessage(m.Content) : m.Content),
                Type = m.Type.ToString(),
                Status = m.Status.ToString(),
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName,
                SenderAvatar = m.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                IsSent = m.SenderId == currentUserId,
                IsDeleted = m.IsDeleted,
                IsEdited = m.IsEdited,
                EditedAt = m.EditedAt,
                CreatedAt = m.CreatedAt,
                DeliveredAt = m.DeliveredAt,
                SeenAt = m.SeenAt,
                ReplyTo = m.ReplyToMessage != null ? new ReplyMessageDto
                {
                    Id = m.ReplyToMessage.Id,
                    Content = m.ReplyToMessage.Content,
                    SenderId = m.ReplyToMessage.SenderId
                } : null,
                Attachments = m.Attachments?.Select(a => new MessageAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    ThumbnailPath = a.ThumbnailPath
                }).ToList()
            }).Reverse();

            return new PagedResultDto<MessageResultDto>
            {
                Data = result.Reverse().ToList(),
                Total = totalMessages,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResultDto<MessageResultDto>> GetGroupMessagesAsync(string currentUserId, int groupId, int page, int pageSize)
        {
            var isMember = await _context.ChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == currentUserId);
            if (!isMember)
                throw new InvalidOperationException("User is not a member of this group");

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

            var result = messages.Select(m => new MessageResultDto
            {
                Id = m.Id,
                Content = m.IsDeleted ? "Tin nhắn đã bị xóa" : (m.IsEncrypted ? DecryptMessage(m.Content) : m.Content),
                Type = m.Type.ToString(),
                Status = m.Status.ToString(),
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName,
                SenderAvatar = m.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                IsSent = m.SenderId == currentUserId,
                IsDeleted = m.IsDeleted,
                IsEdited = m.IsEdited,
                EditedAt = m.EditedAt,
                CreatedAt = m.CreatedAt,
                SeenAt = m.SeenAt,
                ReplyTo = m.ReplyToMessage != null ? new ReplyMessageDto
                {
                    Id = m.ReplyToMessage.Id,
                    Content = m.ReplyToMessage.Content,
                    SenderId = m.ReplyToMessage.SenderId
                } : null,
                Attachments = m.Attachments?.Select(a => new MessageAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    ThumbnailPath = a.ThumbnailPath
                }).ToList()
            }).Reverse();

            return new PagedResultDto<MessageResultDto>
            {
                Data = result.Reverse().ToList(),
                Total = totalMessages,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SendMessageResultDto?> CreateMessageAsync(string currentUserId, SendMessageDto model, List<string>? attachmentPaths = null)
        {
            if (string.IsNullOrEmpty(model.Content) && (attachmentPaths == null || !attachmentPaths.Any()))
                throw new InvalidOperationException("Nội dung tin nhắn không được để trống");

            if (string.IsNullOrEmpty(model.ReceiverId) && model.GroupId == null)
                throw new InvalidOperationException("Phải chỉ định người nhận hoặc nhóm");

            if (!string.IsNullOrEmpty(model.ReceiverId))
            {
                var isBlocked = await IsUserBlockedAsync(currentUserId, model.ReceiverId);
                if (isBlocked)
                    throw new InvalidOperationException("Không thể gửi tin nhắn cho người dùng này");
            }

            if (model.GroupId.HasValue)
            {
                var isMember = await _context.ChatGroupMembers.AnyAsync(m => m.ChatGroupId == model.GroupId && m.UserId == currentUserId);
                if (!isMember)
                    throw new InvalidOperationException("User is not a member of this group");
            }

            var sender = await _userManager.FindByIdAsync(currentUserId);

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

            if (attachmentPaths != null && attachmentPaths.Any())
            {
                message.Attachments = new List<MessageAttachment>();
                foreach (var path in attachmentPaths)
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

            return new SendMessageResultDto
            {
                Id = message.Id,
                Content = model.Content,
                Type = message.Type.ToString(),
                Status = message.Status.ToString(),
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                GroupId = message.ChatGroupId,
                SenderName = sender?.FullName,
                SenderAvatar = sender?.AvatarUrl ?? "/images/default-avatar.png",
                CreatedAt = message.CreatedAt,
                AttachmentPaths = attachmentPaths
            };
        }

        public async Task<bool> MarkAsSeenAsync(string currentUserId, int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return false;

            if (message.ReceiverId != currentUserId)
                return false;

            message.Status = MessageStatus.Seen;
            message.SeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteMessageAsync(string currentUserId, int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return false;

            if (message.SenderId != currentUserId)
                return false;

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsUserBlockedAsync(string userId1, string userId2)
        {
            return await _context.BlockedUsers
                .AnyAsync(b => (b.BlockerId == userId1 && b.BlockedUserId == userId2) ||
                               (b.BlockerId == userId2 && b.BlockedUserId == userId1));
        }

        public async Task BlockUserAsync(string currentUserId, string blockedUserId, string? reason)
        {
            if (string.IsNullOrEmpty(blockedUserId))
                throw new ArgumentException("User ID không hợp lệ");

            var existingBlock = await _context.BlockedUsers
                .FirstOrDefaultAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == blockedUserId);

            if (existingBlock != null)
                throw new InvalidOperationException("Bạn đã chặn người dùng này");

            var blockedUser = new BlockedUser
            {
                BlockerId = currentUserId,
                BlockedUserId = blockedUserId,
                Reason = reason,
                BlockedAt = DateTime.UtcNow
            };

            _context.BlockedUsers.Add(blockedUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UnblockUserAsync(string currentUserId, string blockedUserId)
        {
            var blockedUser = await _context.BlockedUsers
                .FirstOrDefaultAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == blockedUserId);

            if (blockedUser == null)
                return false;

            _context.BlockedUsers.Remove(blockedUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ReportMessageAsync(string currentUserId, int messageId, ReportReason reason, string? description)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                throw new InvalidOperationException("Tin nhắn không tồn tại");

            var report = new MessageReport
            {
                ReporterId = currentUserId,
                MessageId = messageId,
                Reason = reason,
                Description = description,
                Status = ReportStatus.Pending,
                ReportedAt = DateTime.UtcNow
            };

            _context.MessageReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MessageSearchResultDto>> SearchMessagesAsync(string currentUserId, string query, string? userId)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống");

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

            return messages.Select(m => new MessageSearchResultDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                SenderName = m.Sender?.FullName,
                ReceiverName = m.Receiver?.FullName,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<FileUploadResultDto> UploadMessageFileAsync(string currentUserId, IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File quá lớn (tối đa 10MB)");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "messages");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/messages/{uniqueFileName}";

            return new FileUploadResultDto
            {
                FileName = file.FileName,
                FilePath = fileUrl,
                FileType = file.ContentType,
                FileSize = file.Length
            };
        }

        public async Task MarkConversationAsSeenAsync(string currentUserId, string senderId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == currentUserId && m.Status != MessageStatus.Seen)
                .ToListAsync();

            if (!messages.Any()) return;

            var now = DateTime.UtcNow;
            foreach (var msg in messages)
            {
                msg.Status = MessageStatus.Seen;
                msg.SeenAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateMessageStatusToDeliveredAsync(string currentUserId, int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return;

            if (message.ReceiverId != currentUserId) return;

            if (message.Status == MessageStatus.Sent)
            {
                message.Status = MessageStatus.Delivered;
                message.DeliveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CreateGroupAsync(string creatorId, string name, List<string> members)
        {
            var group = new ChatGroup { Name = name };
            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();

            _context.ChatGroupMembers.Add(new ChatGroupMember
            {
                ChatGroupId = group.Id,
                UserId = creatorId,
                Role = GroupRole.Admin
            });

            foreach (var m in members.Distinct().Where(m => m != creatorId))
            {
                _context.ChatGroupMembers.Add(new ChatGroupMember
                {
                    ChatGroupId = group.Id,
                    UserId = m,
                    Role = GroupRole.Member
                });
            }

            await _context.SaveChangesAsync();

            return group.Id;
        }

        private static MessageType ParseMessageType(string? type)
        {
            if (string.IsNullOrEmpty(type)) return MessageType.Text;
            if (Enum.TryParse<MessageType>(type, true, out var result)) return result;
            return MessageType.Text;
        }

        private static string EncryptMessage(string? plainText) => MessageEncryptionHelper.Encrypt(plainText);
        private static string DecryptMessage(string? cipherText) => MessageEncryptionHelper.Decrypt(cipherText);
    }
}
