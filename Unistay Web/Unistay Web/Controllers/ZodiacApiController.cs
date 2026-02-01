using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Unistay_Web.Models;
using Unistay_Web.Services;

namespace Unistay_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZodiacApiController : ControllerBase
    {
        private readonly IZodiacService _zodiacService;

        public ZodiacApiController(IZodiacService zodiacService)
        {
            _zodiacService = zodiacService;
        }

        /// <summary>
        /// GET: api/ZodiacApi
        /// Get all zodiac signs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var signs = await _zodiacService.GetAllZodiacSignsAsync();
                return Ok(new { success = true, data = signs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/5
        /// Get zodiac sign by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var sign = await _zodiacService.GetZodiacSignByIdAsync(id);
                if (sign == null)
                {
                    return NotFound(new { success = false, message = "Zodiac sign not found" });
                }
                return Ok(new { success = true, data = sign });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/name/Aries
        /// Get zodiac sign by English name
        /// </summary>
        [HttpGet("name/{englishName}")]
        public async Task<IActionResult> GetByName(string englishName)
        {
            try
            {
                var sign = await _zodiacService.GetZodiacSignByNameAsync(englishName);
                if (sign == null)
                {
                    return NotFound(new { success = false, message = "Zodiac sign not found" });
                }
                return Ok(new { success = true, data = sign });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/date/2000-03-25
        /// Get zodiac sign by birth date
        /// </summary>
        [HttpGet("date/{dateString}")]
        public async Task<IActionResult> GetByDate(string dateString)
        {
            try
            {
                if (!DateTime.TryParse(dateString, out DateTime birthDate))
                {
                    return BadRequest(new { success = false, message = "Invalid date format" });
                }

                var sign = await _zodiacService.GetZodiacSignByDateAsync(birthDate);
                if (sign == null)
                {
                    return NotFound(new { success = false, message = "Zodiac sign not found for this date" });
                }
                return Ok(new { success = true, data = sign });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/element/Fire
        /// Get zodiac signs by element
        /// </summary>
        [HttpGet("element/{element}")]
        public async Task<IActionResult> GetByElement(string element)
        {
            try
            {
                if (!Enum.TryParse<ZodiacElement>(element, true, out var zodiacElement))
                {
                    return BadRequest(new { success = false, message = "Invalid element. Use: Fire, Earth, Air, or Water" });
                }

                var signs = await _zodiacService.GetZodiacSignsByElementAsync(zodiacElement);
                return Ok(new { success = true, data = signs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/horoscope/5/2026-02-02
        /// Get daily horoscope for a zodiac sign
        /// </summary>
        [HttpGet("horoscope/{zodiacSignId}/{dateString?}")]
        public async Task<IActionResult> GetDailyHoroscope(int zodiacSignId, string? dateString = null)
        {
            try
            {
                DateTime date = DateTime.Today;
                if (!string.IsNullOrEmpty(dateString) && !DateTime.TryParse(dateString, out date))
                {
                    return BadRequest(new { success = false, message = "Invalid date format" });
                }

                var horoscope = await _zodiacService.GetDailyHoroscopeAsync(zodiacSignId, date);
                if (horoscope == null)
                {
                    return NotFound(new { success = false, message = "Horoscope not found" });
                }
                return Ok(new { success = true, data = horoscope });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/horoscope/all/2026-02-02
        /// Get all daily horoscopes for a specific date
        /// </summary>
        [HttpGet("horoscope/all/{dateString?}")]
        public async Task<IActionResult> GetAllDailyHoroscopes(string? dateString = null)
        {
            try
            {
                DateTime date = DateTime.Today;
                if (!string.IsNullOrEmpty(dateString) && !DateTime.TryParse(dateString, out date))
                {
                    return BadRequest(new { success = false, message = "Invalid date format" });
                }

                var horoscopes = await _zodiacService.GetAllDailyHoroscopesAsync(date);
                return Ok(new { success = true, data = horoscopes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/ZodiacApi/compatible/Aries/Leo
        /// Check compatibility between two zodiac signs
        /// </summary>
        [HttpGet("compatible/{sign1}/{sign2}")]
        public async Task<IActionResult> CheckCompatibility(string sign1, string sign2)
        {
            try
            {
                var compatible = await _zodiacService.AreCompatibleAsync(sign1, sign2);
                return Ok(new { success = true, compatible = compatible });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/ZodiacApi/cache/clear
        /// Clear cache
        /// </summary>
        [HttpPost("cache/clear")]
        public IActionResult ClearCache()
        {
            try
            {
                _zodiacService.ClearCache();
                return Ok(new { success = true, message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
