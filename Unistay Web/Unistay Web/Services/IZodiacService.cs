using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unistay_Web.Models;

namespace Unistay_Web.Services
{
    public interface IZodiacService
    {
        /// <summary>
        /// Get all zodiac signs
        /// </summary>
        Task<List<ZodiacSign>> GetAllZodiacSignsAsync();
        
        /// <summary>
        /// Get zodiac sign by ID
        /// </summary>
        Task<ZodiacSign?> GetZodiacSignByIdAsync(int id);
        
        /// <summary>
        /// Get zodiac sign by English name
        /// </summary>
        Task<ZodiacSign?> GetZodiacSignByNameAsync(string englishName);
        
        /// <summary>
        /// Get zodiac sign by birth date
        /// </summary>
        Task<ZodiacSign?> GetZodiacSignByDateAsync(DateTime birthDate);
        
        /// <summary>
        /// Get daily horoscope for a zodiac sign
        /// </summary>
        Task<DailyHoroscope?> GetDailyHoroscopeAsync(int zodiacSignId, DateTime date);
        
        /// <summary>
        /// Get all daily horoscopes for a specific date
        /// </summary>
        Task<List<DailyHoroscope>> GetAllDailyHoroscopesAsync(DateTime date);
        
        /// <summary>
        /// Update daily horoscope
        /// </summary>
        Task<DailyHoroscope> UpdateDailyHoroscopeAsync(DailyHoroscope horoscope);
        
        /// <summary>
        /// Check compatibility between two zodiac signs
        /// </summary>
        Task<bool> AreCompatibleAsync(string sign1, string sign2);
        
        /// <summary>
        /// Get zodiac signs by element
        /// </summary>
        Task<List<ZodiacSign>> GetZodiacSignsByElementAsync(ZodiacElement element);
        
        /// <summary>
        /// Clear cache
        /// </summary>
        void ClearCache();
    }
}
