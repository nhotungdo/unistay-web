using System;
using System.Collections.Generic;

namespace Unistay_Web.Models
{
    public enum ZodiacElement
    {
        Fire = 0,    // Lửa
        Earth = 1,   // Đất
        Air = 2,     // Khí
        Water = 3    // Nước
    }

    public enum ZodiacModality
    {
        Cardinal = 0,  // Khởi đầu
        Fixed = 1,     // Cố định
        Mutable = 2    // Linh hoạt
    }

    public class ZodiacSign
    {
        public int Id { get; set; }
        
        // Basic Information
        public string Name { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty; // Biểu tượng emoji
        public string IconUrl { get; set; } = string.Empty;
        
        // Date Range
        public string DateRange { get; set; } = string.Empty;
        public int StartMonth { get; set; }
        public int StartDay { get; set; }
        public int EndMonth { get; set; }
        public int EndDay { get; set; }
        
        // Astrological Properties
        public ZodiacElement Element { get; set; }
        public ZodiacModality Modality { get; set; }
        public string RulingPlanet { get; set; } = string.Empty;
        public string RulingPlanetVietnamese { get; set; } = string.Empty;
        
        // Characteristics
        public string Description { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public List<string> Traits { get; set; } = new List<string>();
        
        // Compatibility
        public List<string> Compatible { get; set; } = new List<string>();
        public List<string> LessCompatible { get; set; } = new List<string>();
        public string OppositSign { get; set; } = string.Empty;
        
        // Additional Information
        public string SymbolMeaning { get; set; } = string.Empty;
        public string LuckyColor { get; set; } = string.Empty;
        public string LuckyNumber { get; set; } = string.Empty;
        public string LuckyDay { get; set; } = string.Empty;
        
        // Daily Horoscope (Optional - can be fetched from external API)
        public string? DailyHoroscope { get; set; }
        public DateTime? DailyHoroscopeDate { get; set; }
        
        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DailyHoroscope
    {
        public int Id { get; set; }
        public int ZodiacSignId { get; set; }
        public ZodiacSign? ZodiacSign { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; } = string.Empty;
        public string LuckyNumber { get; set; } = string.Empty;
        public string LuckyColor { get; set; } = string.Empty;
        public string Mood { get; set; } = string.Empty;
        public int LoveScore { get; set; } // 0-100
        public int CareerScore { get; set; } // 0-100
        public int HealthScore { get; set; } // 0-100
        public int MoneyScore { get; set; } // 0-100
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
