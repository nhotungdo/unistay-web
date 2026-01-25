using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Unistay_Web.Data;
using Unistay_Web.Models.Chat;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;

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

        public async Task SendMessage(string receiverId, string messageContent, string attachmentUrl = null, string type = "Text")
        {
             var senderId = Context.UserIdentifier;
             if (string.IsNullOrEmpty(senderId)) return;

             // Logic to find or create conversation
             var conversationId = await GetOrCreateConversationId(senderId, receiverId);
             
             // Save Message
             var message = new Message
             {
                 SenderId = senderId,
                 ReceiverId = receiverId,
                 ConversationId = conversationId,
                 Content = messageContent ?? "",
                 AttachmentUrl = attachmentUrl,
                 Type = type,
                 CreatedAt = DateTime.UtcNow,
                 IsRead = false
             };
             _context.Messages.Add(message);
             await _context.SaveChangesAsync();
             
             // Load sender details
             var sender = await _context.Users.FindAsync(senderId);

             var messageDto = new {
                 id = message.Id,
                 senderId = senderId,
                 receiverId = receiverId,
                 senderName = sender?.FullName ?? "Unknown",
                 senderAvatar = sender?.AvatarUrl ?? "/images/default-avatar.png",
                 content = message.Content,
                 attachmentUrl = message.AttachmentUrl,
                 type = message.Type,
                 createdAt = message.CreatedAt,
                 conversationId = conversationId
             };

             // Notify Receiver
             await Clients.User(receiverId).SendAsync("ReceiveMessage", messageDto);
             
             // Notify Sender (ack)
             await Clients.Caller.SendAsync("MessageSent", messageDto);
        }

        public async Task MarkAsRead(int conversationId)
        {
            var userId = Context.UserIdentifier;
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            if (messages.Any())
            {
                foreach (var msg in messages) msg.IsRead = true;
                await _context.SaveChangesAsync();
                
                // Notify sender that messages are read
                var senderId = messages.First().SenderId;
                await Clients.User(senderId).SendAsync("MessagesRead", conversationId);
            }
        }

        private async Task<int> GetOrCreateConversationId(string userId1, string userId2)
        {
            // Try to find existing conversation between these two users
             var conversation = await _context.UserConversations
                 .Where(uc => uc.UserId == userId1)
                 .Select(uc => uc.Conversation)
                 .Where(c => c.UserConversations.Any(uc => uc.UserId == userId2))
                 .FirstOrDefaultAsync();

            if (conversation != null)
            {
                conversation.UpdatedAt = DateTime.UtcNow;
                _context.Entry(conversation).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return conversation.Id;
            }

            // Create new
            var newConv = new Conversation { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Conversations.Add(newConv);
            await _context.SaveChangesAsync();

            _context.UserConversations.Add(new UserConversation { UserId = userId1, ConversationId = newConv.Id });
            _context.UserConversations.Add(new UserConversation { UserId = userId2, ConversationId = newConv.Id });
            await _context.SaveChangesAsync();

            return newConv.Id;
        }
    }
}
