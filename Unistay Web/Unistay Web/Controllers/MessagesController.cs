using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;
using Microsoft.AspNetCore.SignalR;
using Unistay_Web.Hubs;

namespace Unistay_Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(ApplicationDbContext context, UserManager<UserProfile> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpGet("{friendId}")]
        public async Task<IActionResult> GetMessages(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            // Check connection
            var isConnected = await _context.Connections.AnyAsync(c => 
                ((c.RequesterId == currentUserId && c.AddresseeId == friendId) || 
                 (c.RequesterId == friendId && c.AddresseeId == currentUserId)) && 
                c.Status == ConnectionStatus.Accepted);

            if (!isConnected) return BadRequest("Not connected with this user.");

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == friendId) || 
                            (m.SenderId == friendId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp,
                    status = m.Status.ToString().ToLower(),
                    isMine = m.SenderId == currentUserId
                })
                .ToListAsync();

            return Ok(messages);
        }
        
        // Mark all as read for a specific friend
        [HttpPost("read/{friendId}")]
        public async Task<IActionResult> MarkAsRead(string friendId)
        {
             var currentUserId = _userManager.GetUserId(User);
             var unreadMessages = await _context.Messages
                 .Where(m => m.SenderId == friendId && m.ReceiverId == currentUserId && m.Status != MessageStatus.Seen)
                 .ToListAsync();
             
             if (unreadMessages.Any())
             {
                 foreach(var m in unreadMessages) 
                 {
                     m.Status = MessageStatus.Seen;
                     // Notify sender
                     await _hubContext.Clients.Group(m.SenderId).SendAsync("MessageSeen", m.Id);
                 }
                 await _context.SaveChangesAsync();
             }
             return Ok();
        }
    }
}
