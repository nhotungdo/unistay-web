using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Helpers;
using System.IO;
using Unistay_Web.Models.Connection;

namespace Unistay_Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new();
        private static readonly ConcurrentDictionary<string, DateTime> TypingUsers = new();

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Track user connections
                OnlineUsers.AddOrUpdate(userId, 
                    new HashSet<string> { connectionId },
                    (key, existing) => { existing.Add(connectionId); return existing; });

                await Groups.AddToGroupAsync(connectionId, userId);

                try 
                {
                    // Join chat groups - Wrap in try/catch to avoid connection failure if table issue
                    var groupIds = await _db.ChatGroupMembers
                        .Where(m => m.UserId == userId)
                        .Select(m => m.ChatGroupId)
                        .ToListAsync();

                    foreach (var gid in groupIds)
                    {
                        await Groups.AddToGroupAsync(connectionId, $"group_{gid}");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but allow connection to succeed
                    Console.WriteLine($"Error joining groups for user {userId}: {ex.Message}");
                }

                // Notify friends that user is online
                var friendIds = await GetFriendIds(userId);
                foreach (var friendId in friendIds)
                {
                    await Clients.Group(friendId).SendAsync("UserOnline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Remove connection from online users
                if (OnlineUsers.TryGetValue(userId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                        
                        // Notify friends that user is offline
                        var friendIds = await GetFriendIds(userId);
                        foreach (var friendId in friendIds)
                        {
                            await Clients.Group(friendId).SendAsync("UserOffline", userId);
                        }
                    }
                }

                await Groups.RemoveFromGroupAsync(connectionId, userId);

                try 
                {
                    var groupIds = await _db.ChatGroupMembers
                         .Where(m => m.UserId == userId)
                         .Select(m => m.ChatGroupId)
                         .ToListAsync();

                    foreach (var gid in groupIds)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, $"group_{gid}");
                    }
                }
                catch 
                { 
                    // Ignore errors during disconnect 
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string? receiverId, int? groupId, string content, List<string>? attachmentPaths = null, string? type = null, int? replyToMessageId = null, bool isEncrypted = false)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId)) return;

            // Group message
            if (groupId.HasValue)
            {
                var member = await _db.ChatGroupMembers
                    .FirstOrDefaultAsync(m => m.ChatGroupId == groupId.Value && m.UserId == senderId);
                if (member == null) return;

                var msg = new Message
                {
                    SenderId = senderId,
                    ChatGroupId = groupId,
                    Content = isEncrypted ? MessageEncryptionHelper.Encrypt(content) : content, // Encrypt if flagged, or basic text
                    Type = ParseMessageType(type),
                    Status = MessageStatus.Sent,
                    IsEncrypted = isEncrypted,
                    ReplyToMessageId = replyToMessageId,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Attachments
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    msg.Attachments = new List<MessageAttachment>();
                    foreach (var path in attachmentPaths)
                    {
                        msg.Attachments.Add(new MessageAttachment
                        {
                             FilePath = path,
                             FileName = Path.GetFileName(path), 
                             FileType = "file", 
                             FileSize = 0 
                        });
                        if (msg.Type == MessageType.Text) msg.Type = MessageType.File;
                    }
                }

                _db.Messages.Add(msg);
                await _db.SaveChangesAsync();

                // Load sender info
                await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();

                var messageData = new
                {
                    id = msg.Id,
                    content = content, // Send back plain text to group (Server decodes/echoes)
                    type = msg.Type.ToString(),
                    status = msg.Status.ToString(),
                    senderId = msg.SenderId,
                    senderName = msg.Sender?.FullName,
                    senderAvatar = msg.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                    createdAt = msg.CreatedAt,
                    groupId = msg.ChatGroupId,
                    attachmentPaths = attachmentPaths,
                    replyTo = replyToMessageId
                };

                await Clients.Group($"group_{groupId.Value}").SendAsync("ReceiveMessage", messageData);
                return;
            }

            // Direct message
            if (!string.IsNullOrEmpty(receiverId))
            {
                // Check if blocked
                var isBlocked = await _db.BlockedUsers
                    .AnyAsync(b => (b.BlockerId == senderId && b.BlockedUserId == receiverId) ||
                                  (b.BlockerId == receiverId && b.BlockedUserId == senderId));
                
                if (isBlocked) return;

                var msg = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = isEncrypted ? MessageEncryptionHelper.Encrypt(content) : content,
                    Type = ParseMessageType(type),
                    Status = MessageStatus.Sent,
                    IsEncrypted = isEncrypted,
                    ReplyToMessageId = replyToMessageId,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Attachments
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    msg.Attachments = new List<MessageAttachment>();
                    foreach (var path in attachmentPaths)
                    {
                        msg.Attachments.Add(new MessageAttachment
                        {
                             FilePath = path,
                             FileName = Path.GetFileName(path), 
                             FileType = "file", 
                             FileSize = 0 
                        });
                        if (msg.Type == MessageType.Text) msg.Type = MessageType.File;
                    }
                }

                _db.Messages.Add(msg);
                await _db.SaveChangesAsync();

                // Load sender info
                await _db.Entry(msg).Reference(m => m.Sender).LoadAsync();

                var messageData = new
                {
                    id = msg.Id,
                    content = content,
                    type = msg.Type.ToString(),
                    status = msg.Status.ToString(),
                    senderId = msg.SenderId,
                    receiverId = msg.ReceiverId,
                    senderName = msg.Sender?.FullName,
                    senderAvatar = msg.Sender?.AvatarUrl ?? "/images/default-avatar.png",
                    createdAt = msg.CreatedAt,
                    attachmentPaths = attachmentPaths,
                    replyTo = replyToMessageId
                };

                // Send to receiver
                await Clients.Group(receiverId).SendAsync("ReceiveMessage", messageData);
                
                // Confirm to sender
                await Clients.Caller.SendAsync("MessageSent", messageData);

                // Check if receiver is online and auto-mark as delivered
                if (IsUserOnline(receiverId))
                {
                    msg.Status = MessageStatus.Delivered;
                    msg.DeliveredAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                    
                    await Clients.Group(senderId).SendAsync("MessageDelivered", msg.Id);
                }
            }
        }

        public async Task Typing(string receiverId, int? groupId)
        {
            var sender = Context.UserIdentifier;
            if (string.IsNullOrEmpty(sender)) return;

            var typingKey = $"{sender}_{receiverId ?? groupId?.ToString()}";
            TypingUsers[typingKey] = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(receiverId))
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", sender);
            }
            else if (groupId.HasValue)
            {
                await Clients.Group($"group_{groupId.Value}").SendAsync("UserTyping", sender);
            }

            // Auto stop typing after 3 seconds
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (TypingUsers.TryGetValue(typingKey, out var lastTyping))
                {
                    if ((DateTime.UtcNow - lastTyping).TotalSeconds >= 3)
                    {
                        TypingUsers.TryRemove(typingKey, out _);
                        await StopTyping(receiverId, groupId);
                    }
                }
            });
        }

        public async Task StopTyping(string? receiverId, int? groupId)
        {
            var sender = Context.UserIdentifier;
            if (string.IsNullOrEmpty(sender)) return;

            if (!string.IsNullOrEmpty(receiverId))
            {
                await Clients.Group(receiverId).SendAsync("UserStoppedTyping", sender);
            }
            else if (groupId.HasValue)
            {
                await Clients.Group($"group_{groupId.Value}").SendAsync("UserStoppedTyping", sender);
            }
        }

        public async Task MessageRead(int messageId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var msg = await _db.Messages.FindAsync(messageId);
            if (msg == null || msg.ReceiverId != userId) return;

            msg.Status = MessageStatus.Seen;
            msg.SeenAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(msg.SenderId))
            {
                await Clients.Group(msg.SenderId).SendAsync("MessageSeen", new { messageId = msg.Id, seenAt = msg.SeenAt });
            }
        }

        public async Task MessageDelivered(int messageId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var msg = await _db.Messages.FindAsync(messageId);
            if (msg == null || msg.ReceiverId != userId) return;

            if (msg.Status == MessageStatus.Sent)
            {
                msg.Status = MessageStatus.Delivered;
                msg.DeliveredAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(msg.SenderId))
                {
                    await Clients.Group(msg.SenderId).SendAsync("MessageDelivered", msg.Id);
                }
            }
        }

        public async Task MarkConversationAsSeen(string senderId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var messages = await _db.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == userId && m.Status != MessageStatus.Seen)
                .ToListAsync();

            if (!messages.Any()) return;

            var now = DateTime.UtcNow;
            foreach (var msg in messages)
            {
                msg.Status = MessageStatus.Seen;
                msg.SeenAt = now;
            }

            await _db.SaveChangesAsync();

            await Clients.Group(senderId).SendAsync("ConversationSeen", new { userId = userId, seenAt = now });
        }

        public async Task CreateGroup(string name, List<string> members)
        {
            var creator = Context.UserIdentifier;
            if (string.IsNullOrEmpty(creator)) return;

            var group = new ChatGroup { Name = name };
            _db.ChatGroups.Add(group);
            await _db.SaveChangesAsync();

            // Add creator as admin
            _db.ChatGroupMembers.Add(new ChatGroupMember 
            { 
                ChatGroupId = group.Id, 
                UserId = creator, 
                Role = GroupRole.Admin 
            });

            foreach (var m in members.Distinct().Where(m => m != creator))
            {
                _db.ChatGroupMembers.Add(new ChatGroupMember 
                { 
                    ChatGroupId = group.Id, 
                    UserId = m, 
                    Role = GroupRole.Member 
                });
            }

            await _db.SaveChangesAsync();

            // Add all members to SignalR group
            var allMembers = members.Concat(new[] { creator }).Distinct();
            foreach (var memberId in allMembers)
            {
                if (OnlineUsers.ContainsKey(memberId))
                {
                    foreach (var connId in OnlineUsers[memberId])
                    {
                        await Groups.AddToGroupAsync(connId, $"group_{group.Id}");
                    }
                    await Clients.Group(memberId).SendAsync("AddedToGroup", new { groupId = group.Id, groupName = name });
                }
            }
        }

        public async Task GetOnlineStatus(List<string> userIds)
        {
            var onlineStatuses = userIds.Select(id => new
            {
                userId = id,
                isOnline = IsUserOnline(id)
            }).ToList();

            await Clients.Caller.SendAsync("OnlineStatuses", onlineStatuses);
        }

        // Helper methods
        private async Task<List<string>> GetFriendIds(string userId)
        {
            var friends = await _db.Connections
                .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) 
                    && c.Status == ConnectionStatus.Accepted)
                .Select(c => c.RequesterId == userId ? c.AddresseeId : c.RequesterId)
                .ToListAsync();

            return friends.Where(f => f != null).Cast<string>().ToList();
        }

        private static bool IsUserOnline(string userId)
        {
            return OnlineUsers.ContainsKey(userId) && OnlineUsers[userId].Count > 0;
        }

        private static MessageType ParseMessageType(string? type)
        {
            if (string.IsNullOrEmpty(type)) return MessageType.Text;
            if (Enum.TryParse<MessageType>(type, true, out var result)) return result;
            return MessageType.Text;
        }
    }
}
