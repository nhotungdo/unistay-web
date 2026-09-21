using Microsoft.AspNetCore.Mvc;

namespace UnistayWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetFavorites()
        {
            return Ok(new { data = new List<object>() });
        }

        [HttpPost("add/{roomId}")]
        public IActionResult Add(int roomId)
        {
            // TODO: Add to favorites
            return Ok(new { success = true, message = "Đã thêm vào danh sách yêu thích" });
        }

        [HttpDelete("remove/{roomId}")]
        public IActionResult Remove(int roomId)
        {
            // TODO: Remove from favorites
            return Ok(new { success = true, message = "Đã xóa khỏi danh sách yêu thích" });
        }
    }
}
