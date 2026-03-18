using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;

namespace Unistay_Web.Controllers
{
    [Authorize]
    [Route("Messages")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserProfile> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<UserProfile> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index([FromQuery] string? friendId = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return RedirectToAction("Login", "Account");

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SelectedFriendId = friendId ?? string.Empty;

            // Load friends (accepted connections)
            try
            {
                var friendIds = await _context.Connections
                    .Where(c => (c.RequesterId == currentUserId || c.AddresseeId == currentUserId)
                                && c.Status == ConnectionStatus.Accepted)
                    .Select(c => c.RequesterId == currentUserId ? c.AddresseeId : c.RequesterId)
                    .ToListAsync();

                var friends = await _context.Users
                    .Where(u => friendIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
                    .ToListAsync();

                ViewBag.Friends = friends;
            }
            catch
            {
                ViewBag.Friends = new List<object>();
            }

            // Load groups
            try
            {
                var groupIds = await _context.ChatGroupMembers
                    .Where(m => m.UserId == currentUserId)
                    .Select(m => m.ChatGroupId)
                    .ToListAsync();

                var groups = await _context.ChatGroups
                    .Where(g => groupIds.Contains(g.Id))
                    .Select(g => new { g.Id, g.Name })
                    .ToListAsync();

                ViewBag.Groups = groups;
            }
            catch
            {
                ViewBag.Groups = new List<object>();
            }

            return View("~/Views/Chat/Index.cshtml");
        }
    }
}
