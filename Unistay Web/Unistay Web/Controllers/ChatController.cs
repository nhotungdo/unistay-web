using Microsoft.AspNetCore.Mvc;

namespace Unistay_Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Conversation(string userId)
        {
            return View();
        }
    }
}
