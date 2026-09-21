using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnistayWeb.DAL.Models.RentalAdvice;
using UnistayWeb.BLL.Services.RentalAdvice;
using UnistayWeb.DAL.Data;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalAdviceController : ControllerBase
    {
        private readonly IRentalAdviceService _adviceService;
        private readonly ApplicationDbContext _context;

        public RentalAdviceController(IRentalAdviceService adviceService, ApplicationDbContext context)
        {
            _adviceService = adviceService;
            _context = context;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> GetChatResponse([FromBody] ChatRequest request)
        {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (userId == null) return Unauthorized();
             
             var response = await _adviceService.GetChatResponseAsync(request.Message, userId);
             return Ok(new { response });
        }

        [HttpGet("neighborhoods")]
        public async Task<IActionResult> Neighborhoods()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            
            var areas = await _adviceService.GetNeighborhoodSuggestionsAsync(userId);
            return Ok(areas);
        }

        [HttpGet("risk-analysis/{id?}")]
        public async Task<IActionResult> RiskAnalysis(int id)
        {
            var analysis = await _adviceService.AnalyzeRoomRisksAsync(id);
            return Ok(analysis);
        }

        [HttpGet("price-analysis")]
        public async Task<IActionResult> PriceAnalysis([FromQuery] string? area)
        {
            if (string.IsNullOrEmpty(area)) area = "Downtown"; // Default
            var trends = await _adviceService.GetPriceTrendsAsync(area);
            return Ok(trends);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> Compare([FromQuery] string? ids)
        {
            if (string.IsNullOrEmpty(ids)) 
            {
                return Ok(new List<UnistayWeb.DAL.Models.Room.Room>());
            }
            
            var idList = ids.Split(',').Select(int.Parse).ToList();
            var rooms = await _context.Rooms.Where(r => idList.Contains(r.Id)).ToListAsync();
            
            return Ok(rooms);
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
