using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using System.Security.Claims;
using Unistay_Web.Models.User;

namespace Unistay_Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                
                // Notify friends that user is online
                var friendIds = await _context.Connections
                    .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
                    .Select(c => c.RequesterId == userId ? c.AddresseeId : c.RequesterId)
                    .ToListAsync();

                if (friendIds.Any())
                {
                    await Clients.Groups(friendIds).SendAsync("UserOnline", userId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                
                // Notify friends that user is offline
                var friendIds = await _context.Connections
                    .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
                    .Select(c => c.RequesterId == userId ? c.AddresseeId : c.RequesterId)
                    .ToListAsync();

                if (friendIds.Any())
                {
                    await Clients.Groups(friendIds).SendAsync("UserOffline", userId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string content)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(content))
                return;

            // Verify connection
            var isConnected = await _context.Connections.AnyAsync(c => 
                ((c.RequesterId == senderId && c.AddresseeId == receiverId) || 
                 (c.RequesterId == receiverId && c.AddresseeId == senderId)) && 
                c.Status == ConnectionStatus.Accepted);

            if (!isConnected)
            {
                return;
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Send to receiver
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", new {
                id = message.Id,
                senderId = message.SenderId,
                content = message.Content,
                timestamp = message.Timestamp,
                status = "sent"
            });

            // Send confirmation to sender
            await Clients.Group(senderId).SendAsync("MessageSent", new {
                id = message.Id,
                receiverId = message.ReceiverId,
                content = message.Content,
                timestamp = message.Timestamp,
                status = "sent"
            });
        }

        public async Task Typing(string receiverId)
        {
            var senderId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(senderId))
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", senderId);
            }
        }

        public async Task StopTyping(string receiverId)
        {
            var senderId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(senderId))
            {
                await Clients.Group(receiverId).SendAsync("UserStoppedTyping", senderId);
            }
        }
        
        public async Task MessageRead(int messageId)
        {
             var userId = Context.UserIdentifier;
             var message = await _context.Messages.FindAsync(messageId);
             if (message != null && message.ReceiverId == userId && message.Status != MessageStatus.Seen)
             {
                 message.Status = MessageStatus.Seen;
                 await _context.SaveChangesAsync();
                 
                 await Clients.Group(message.SenderId).SendAsync("MessageSeen", messageId);
             }
        }

        public async Task MessageDelivered(int messageId)
        {
             var userId = Context.UserIdentifier;
             var message = await _context.Messages.FindAsync(messageId);
             if (message != null && message.ReceiverId == userId && message.Status == MessageStatus.Sent)
             {
                 message.Status = MessageStatus.Delivered;
                 await _context.SaveChangesAsync();
                 
                 await Clients.Group(message.SenderId).SendAsync("MessageDelivered", messageId);
             }
        }
    }
}
