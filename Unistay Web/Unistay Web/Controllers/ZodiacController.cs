using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Unistay_Web.Services;

namespace Unistay_Web.Controllers
{
    [Route("cung-hoang-dao")]
    public class ZodiacController : Controller
    {
        private readonly IZodiacService _zodiacService;

        public ZodiacController(IZodiacService zodiacService)
        {
            _zodiacService = zodiacService;
        }

        /// <summary>
        /// GET: /cung-hoang-dao
        /// Display all zodiac signs
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var signs = await _zodiacService.GetAllZodiacSignsAsync();
                ViewData["Title"] = "12 Cung Hoàng Đạo";
                return View(signs);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// GET: /cung-hoang-dao/chi-tiet/5
        /// Display zodiac sign details
        /// </summary>
        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sign = await _zodiacService.GetZodiacSignByIdAsync(id);
                if (sign == null)
                {
                    return NotFound();
                }

                // Get today's horoscope
                var horoscope = await _zodiacService.GetDailyHoroscopeAsync(id, DateTime.Today);
                ViewBag.DailyHoroscope = horoscope;
                
                ViewData["Title"] = $"Cung {sign.Name}";
                return View(sign);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// GET: /cung-hoang-dao/tim-kiem
        /// Find your zodiac sign
        /// </summary>
        [HttpGet("tim-kiem")]
        public IActionResult FindYourSign()
        {
            ViewData["Title"] = "Tìm Cung Hoàng Đạo Của Bạn";
            return View();
        }

        /// <summary>
        /// POST: /cung-hoang-dao/tim-kiem
        /// Process zodiac sign search
        /// </summary>
        [HttpPost("tim-kiem")]
        public async Task<IActionResult> FindYourSign(DateTime birthDate)
        {
            try
            {
                var sign = await _zodiacService.GetZodiacSignByDateAsync(birthDate);
                if (sign == null)
                {
                    ViewBag.Message = "Không tìm thấy cung hoàng đạo cho ngày sinh này.";
                    return View();
                }

                return RedirectToAction("Details", new { id = sign.Id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        /// <summary>
        /// GET: /cung-hoang-dao/tu-vi-hang-ngay
        /// Display daily horoscopes for all signs
        /// </summary>
        [HttpGet("tu-vi-hang-ngay")]
        public async Task<IActionResult> DailyHoroscope()
        {
            try
            {
                var horoscopes = await _zodiacService.GetAllDailyHoroscopesAsync(DateTime.Today);
                ViewData["Title"] = "Tử Vi Hàng Ngày";
                ViewBag.Date = DateTime.Today;
                return View(horoscopes);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }

        /// <summary>
        /// GET: /cung-hoang-dao/tuong-thich
        /// Check compatibility between zodiac signs
        /// </summary>
        [HttpGet("tuong-thich")]
        public async Task<IActionResult> Compatibility()
        {
            var signs = await _zodiacService.GetAllZodiacSignsAsync();
            ViewData["Title"] = "Xem Độ Tương Thích";
            return View(signs);
        }
    }
}
