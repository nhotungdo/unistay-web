using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unistay_Web.Models.RentalAdvice;
using Unistay_Web.Services.RentalAdvice;
using Unistay_Web.Data; // Assuming context might be needed for direct simpler queries or passing to views

namespace Unistay_Web.Controllers
{
    [Route("RentalAdvice")]
    public class RentalAdviceController : Controller
    {
        private readonly IRentalAdviceService _adviceService;
        private readonly ApplicationDbContext _context;

        public RentalAdviceController(IRentalAdviceService adviceService, ApplicationDbContext context)
        {
            _adviceService = adviceService;
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");
            
            ViewBag.AIAdvice = await _adviceService.GetPersonalizedAdviceAsync(userId);
            
            // Get some initial suggestions for the dashboard
            var rooms = await _adviceService.GetRoomSuggestionsAsync(userId);
            return View(rooms);
        }

        [Route("Chat")]
        public IActionResult Chat()
        {
            return View();
        }

        [HttpPost("GetChatResponse")]
        public async Task<IActionResult> GetChatResponse([FromBody] ChatRequest request)
        {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (userId == null) return Unauthorized();
             
             var response = await _adviceService.GetChatResponseAsync(request.Message, userId);
             return Json(new { response });
        }

        [Route("Neighborhoods")]
        public async Task<IActionResult> Neighborhoods()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");
            
            var areas = await _adviceService.GetNeighborhoodSuggestionsAsync(userId);
            return View(areas);
        }

        [Route("RiskAnalysis/{id?}")]
        public async Task<IActionResult> RiskAnalysis(int id)
        {
            var analysis = await _adviceService.AnalyzeRoomRisksAsync(id);
            return View(analysis);
        }

        [Route("PriceAnalysis")]
        public async Task<IActionResult> PriceAnalysis(string area)
        {
            if (string.IsNullOrEmpty(area)) area = "Downtown"; // Default
            var trends = await _adviceService.GetPriceTrendsAsync(area);
            return View(trends);
        }

        [Route("Compare")]
        public async Task<IActionResult> Compare(string ids)
        {
            if (string.IsNullOrEmpty(ids)) 
            {
                return View(new List<Unistay_Web.Models.Room.Room>());
            }
            
            var idList = ids.Split(',').Select(int.Parse).ToList();
            var rooms = _context.Rooms.Where(r => idList.Contains(r.Id)).ToList();
            
            return View(rooms);
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
