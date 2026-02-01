using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Unistay_Web.Models;

namespace Unistay_Web.Services
{
    public class ZodiacService : IZodiacService
    {
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY_ALL_SIGNS = "AllZodiacSigns";
        private const string CACHE_KEY_HOROSCOPE_PREFIX = "DailyHoroscope_";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);
        private readonly TimeSpan _horoscopeCacheExpiration = TimeSpan.FromHours(6);

        public ZodiacService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<ZodiacSign>> GetAllZodiacSignsAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue(CACHE_KEY_ALL_SIGNS, out List<ZodiacSign>? cachedSigns) && cachedSigns != null)
            {
                return cachedSigns;
            }

            // If not in cache, load from data source
            var signs = await LoadZodiacSignsAsync();
            
            // Store in cache
            _cache.Set(CACHE_KEY_ALL_SIGNS, signs, _cacheExpiration);
            
            return signs;
        }

        public async Task<ZodiacSign?> GetZodiacSignByIdAsync(int id)
        {
            var signs = await GetAllZodiacSignsAsync();
            return signs.FirstOrDefault(s => s.Id == id);
        }

        public async Task<ZodiacSign?> GetZodiacSignByNameAsync(string englishName)
        {
            var signs = await GetAllZodiacSignsAsync();
            return signs.FirstOrDefault(s => 
                s.EnglishName.Equals(englishName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ZodiacSign?> GetZodiacSignByDateAsync(DateTime birthDate)
        {
            var signs = await GetAllZodiacSignsAsync();
            int month = birthDate.Month;
            int day = birthDate.Day;

            foreach (var sign in signs)
            {
                if (IsDateInRange(month, day, sign.StartMonth, sign.StartDay, sign.EndMonth, sign.EndDay))
                {
                    return sign;
                }
            }

            return null;
        }

        public async Task<DailyHoroscope?> GetDailyHoroscopeAsync(int zodiacSignId, DateTime date)
        {
            string cacheKey = $"{CACHE_KEY_HOROSCOPE_PREFIX}{zodiacSignId}_{date:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out DailyHoroscope? cachedHoroscope) && cachedHoroscope != null)
            {
                return cachedHoroscope;
            }

            // Generate or fetch horoscope
            var horoscope = await GenerateDailyHoroscopeAsync(zodiacSignId, date);
            
            if (horoscope != null)
            {
                _cache.Set(cacheKey, horoscope, _horoscopeCacheExpiration);
            }
            
            return horoscope;
        }

        public async Task<List<DailyHoroscope>> GetAllDailyHoroscopesAsync(DateTime date)
        {
            var signs = await GetAllZodiacSignsAsync();
            var horoscopes = new List<DailyHoroscope>();

            foreach (var sign in signs)
            {
                var horoscope = await GetDailyHoroscopeAsync(sign.Id, date);
                if (horoscope != null)
                {
                    horoscopes.Add(horoscope);
                }
            }

            return horoscopes;
        }

        public async Task<DailyHoroscope> UpdateDailyHoroscopeAsync(DailyHoroscope horoscope)
        {
            // In a real application, this would update the database
            // For now, we'll just update the cache
            string cacheKey = $"{CACHE_KEY_HOROSCOPE_PREFIX}{horoscope.ZodiacSignId}_{horoscope.Date:yyyyMMdd}";
            _cache.Set(cacheKey, horoscope, _horoscopeCacheExpiration);
            
            return await Task.FromResult(horoscope);
        }

        public async Task<bool> AreCompatibleAsync(string sign1, string sign2)
        {
            var zodiac1 = await GetZodiacSignByNameAsync(sign1);
            if (zodiac1 == null) return false;

            return zodiac1.Compatible.Any(c => c.Equals(sign2, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<ZodiacSign>> GetZodiacSignsByElementAsync(ZodiacElement element)
        {
            var signs = await GetAllZodiacSignsAsync();
            return signs.Where(s => s.Element == element).ToList();
        }

        public void ClearCache()
        {
            _cache.Remove(CACHE_KEY_ALL_SIGNS);
        }

        #region Private Helper Methods

        private bool IsDateInRange(int month, int day, int startMonth, int startDay, int endMonth, int endDay)
        {
            if (startMonth == endMonth)
            {
                return month == startMonth && day >= startDay && day <= endDay;
            }
            else if (startMonth < endMonth)
            {
                return (month == startMonth && day >= startDay) || 
                       (month == endMonth && day <= endDay) ||
                       (month > startMonth && month < endMonth);
            }
            else // Crosses year boundary (e.g., Capricorn: Dec 22 - Jan 19)
            {
                return (month == startMonth && day >= startDay) || 
                       (month == endMonth && day <= endDay) ||
                       (month > startMonth || month < endMonth);
            }
        }

        private async Task<List<ZodiacSign>> LoadZodiacSignsAsync()
        {
            // Data from the comprehensive research
            return await Task.FromResult(new List<ZodiacSign>
            {
                new ZodiacSign
                {
                    Id = 1,
                    Name = "Bạch Dương",
                    EnglishName = "Aries",
                    Symbol = "♈",
                    IconUrl = "/images/zodiac/aries.svg",
                    DateRange = "21/3 – 19/4",
                    StartMonth = 3, StartDay = 21,
                    EndMonth = 4, EndDay = 19,
                    Element = ZodiacElement.Fire,
                    Modality = ZodiacModality.Cardinal,
                    RulingPlanet = "Mars",
                    RulingPlanetVietnamese = "Sao Hỏa",
                    Description = "Bạch Dương là cung hoàng đạo đầu tiên, tượng trưng cho sự khởi đầu, năng lượng mới mẻ và tinh thần tiên phong. Họ là những người thẳng thắn, chân thật, ấm áp và chính trực.",
                    Strengths = new List<string> { "Lãnh đạo bẩm sinh", "Nhiệt tình", "Kiên định", "Dũng cảm", "Coi trọng tình cảm" },
                    Weaknesses = new List<string> { "Nóng nảy", "Bốc đồng", "Thiếu kiên nhẫn", "Bướng bỉnh" },
                    Traits = new List<string> { "Năng động", "Nhiệt huyết", "Dũng cảm", "Sẵn sàng đối mặt thử thách" },
                    Compatible = new List<string> { "Leo", "Sagittarius", "Gemini", "Aquarius" },
                    LessCompatible = new List<string> { "Cancer", "Capricorn" },
                    OppositSign = "Libra",
                    SymbolMeaning = "Con cừu tượng trưng cho sự mạnh mẽ, quyết tâm và tinh thần chiến đấu",
                    LuckyColor = "Đỏ",
                    LuckyNumber = "1, 9",
                    LuckyDay = "Thứ Ba"
                },
                new ZodiacSign
                {
                    Id = 2,
                    Name = "Kim Ngưu",
                    EnglishName = "Taurus",
                    Symbol = "♉",
                    IconUrl = "/images/zodiac/taurus.svg",
                    DateRange = "20/4 – 20/5",
                    StartMonth = 4, StartDay = 20,
                    EndMonth = 5, EndDay = 20,
                    Element = ZodiacElement.Earth,
                    Modality = ZodiacModality.Fixed,
                    RulingPlanet = "Venus",
                    RulingPlanetVietnamese = "Sao Kim",
                    Description = "Kim Ngưu thường thật thà, lương thiện và dễ mềm lòng. Họ là những người kiên nhẫn, đáng tin cậy, thực tế và yêu thích sự ổn định.",
                    Strengths = new List<string> { "Kiên định", "Đáng tin cậy", "Chung thủy", "Thực tế", "Đánh giá cao cái đẹp" },
                    Weaknesses = new List<string> { "Cứng nhắc", "Bướng bỉnh", "Chiếm hữu", "Bảo thủ", "Ham vật chất" },
                    Traits = new List<string> { "Kiên nhẫn", "Đáng tin cậy", "Thực tế", "Yêu sự ổn định" },
                    Compatible = new List<string> { "Virgo", "Capricorn", "Cancer", "Pisces" },
                    LessCompatible = new List<string> { "Leo", "Aquarius" },
                    OppositSign = "Scorpio",
                    SymbolMeaning = "Con bò tót tượng trưng cho sức mạnh, sự kiên định và bền bỉ",
                    LuckyColor = "Xanh lá, Hồng",
                    LuckyNumber = "2, 6",
                    LuckyDay = "Thứ Sáu"
                },
                new ZodiacSign
                {
                    Id = 3,
                    Name = "Song Tử",
                    EnglishName = "Gemini",
                    Symbol = "♊",
                    IconUrl = "/images/zodiac/gemini.svg",
                    DateRange = "21/5 – 21/6",
                    StartMonth = 5, StartDay = 21,
                    EndMonth = 6, EndDay = 21,
                    Element = ZodiacElement.Air,
                    Modality = ZodiacModality.Mutable,
                    RulingPlanet = "Mercury",
                    RulingPlanetVietnamese = "Sao Thủy",
                    Description = "Song Tử được biết đến là người thông minh, sáng tạo, linh hoạt và có óc suy luận sắc bén. Họ thích khám phá và có khả năng giải quyết tình huống một cách hợp lý.",
                    Strengths = new List<string> { "Thích nghi nhanh", "Giao tiếp tốt", "Hài hước", "Thông minh", "Sáng tạo" },
                    Weaknesses = new List<string> { "Không ổn định", "Dễ thay đổi", "Thiếu quyết đoán", "Thiếu kiên định" },
                    Traits = new List<string> { "Linh hoạt", "Sáng tạo", "Thông minh", "Hòa đồng", "Tò mò" },
                    Compatible = new List<string> { "Libra", "Aquarius", "Aries", "Leo" },
                    LessCompatible = new List<string> { "Virgo", "Pisces" },
                    OppositSign = "Sagittarius",
                    SymbolMeaning = "Cặp song sinh tượng trưng cho tính hai mặt và khả năng nhìn nhận đa chiều",
                    LuckyColor = "Vàng",
                    LuckyNumber = "5, 7",
                    LuckyDay = "Thứ Tư"
                },
                new ZodiacSign
                {
                    Id = 4,
                    Name = "Cự Giải",
                    EnglishName = "Cancer",
                    Symbol = "♋",
                    IconUrl = "/images/zodiac/cancer.svg",
                    DateRange = "22/6 – 22/7",
                    StartMonth = 6, StartDay = 22,
                    EndMonth = 7, EndDay = 22,
                    Element = ZodiacElement.Water,
                    Modality = ZodiacModality.Cardinal,
                    RulingPlanet = "Moon",
                    RulingPlanetVietnamese = "Mặt Trăng",
                    Description = "Cự Giải là những người giàu cảm xúc, khả năng chăm sóc và bảo vệ người thân, trọng tình cảm. Mặt Trăng ảnh hưởng mạnh mẽ đến cảm xúc của họ.",
                    Strengths = new List<string> { "Tình cảm", "Quan tâm người khác", "Trung thành", "Trực giác tốt", "Nuôi dưỡng" },
                    Weaknesses = new List<string> { "Dễ tổn thương", "Nhạy cảm", "Thiếu mạnh mẽ", "Hay buồn bã" },
                    Traits = new List<string> { "Giàu cảm xúc", "Chăm sóc", "Bảo vệ", "Trọng tình cảm" },
                    Compatible = new List<string> { "Pisces", "Scorpio", "Taurus", "Virgo" },
                    LessCompatible = new List<string> { "Aries", "Libra" },
                    OppositSign = "Capricorn",
                    SymbolMeaning = "Con cua với vỏ cứng bên ngoài nhưng mềm mại bên trong",
                    LuckyColor = "Trắng, Bạc",
                    LuckyNumber = "2, 7",
                    LuckyDay = "Thứ Hai, Thứ Năm"
                },
                new ZodiacSign
                {
                    Id = 5,
                    Name = "Sư Tử",
                    EnglishName = "Leo",
                    Symbol = "♌",
                    IconUrl = "/images/zodiac/leo.svg",
                    DateRange = "23/7 – 22/8",
                    StartMonth = 7, StartDay = 23,
                    EndMonth = 8, EndDay = 22,
                    Element = ZodiacElement.Fire,
                    Modality = ZodiacModality.Fixed,
                    RulingPlanet = "Sun",
                    RulingPlanetVietnamese = "Mặt Trời",
                    Description = "Sư Tử tự tin, nhiệt thành, có tố chất lãnh đạo bẩm sinh và thích được chú ý. Mặt Trời phản ánh mong muốn của Sư Tử là trung tâm của sự chú ý.",
                    Strengths = new List<string> { "Lãnh đạo", "Thu hút", "Tự tin", "Hào phóng", "Sáng tạo" },
                    Weaknesses = new List<string> { "Kiêu ngạo", "Thiếu kiểm soát", "Áp đặt", "Ái kỷ", "Coi trọng thể diện" },
                    Traits = new List<string> { "Tự tin", "Nhiệt thành", "Lãnh đạo", "Thích chú ý" },
                    Compatible = new List<string> { "Aries", "Sagittarius", "Gemini", "Libra" },
                    LessCompatible = new List<string> { "Taurus", "Scorpio" },
                    OppositSign = "Aquarius",
                    SymbolMeaning = "Sư tử - vua của muôn loài, tượng trưng cho sức mạnh và uy quyền",
                    LuckyColor = "Vàng, Cam",
                    LuckyNumber = "1, 3, 10, 19",
                    LuckyDay = "Chủ Nhật"
                },
                new ZodiacSign
                {
                    Id = 6,
                    Name = "Xử Nữ",
                    EnglishName = "Virgo",
                    Symbol = "♍",
                    IconUrl = "/images/zodiac/virgo.svg",
                    DateRange = "23/8 – 22/9",
                    StartMonth = 8, StartDay = 23,
                    EndMonth = 9, EndDay = 22,
                    Element = ZodiacElement.Earth,
                    Modality = ZodiacModality.Mutable,
                    RulingPlanet = "Mercury",
                    RulingPlanetVietnamese = "Sao Thủy",
                    Description = "Xử Nữ cẩn trọng, chu đáo, có trách nhiệm và chú ý đến chi tiết. Họ theo đuổi sự hoàn hảo trong mọi việc.",
                    Strengths = new List<string> { "Tỉ mỉ", "Có tổ chức", "Thông minh", "Thực tế", "Phân tích tốt" },
                    Weaknesses = new List<string> { "Hay lo lắng", "Chỉ trích", "Khó tính", "Cầu toàn", "Thiếu bao dung" },
                    Traits = new List<string> { "Cẩn trọng", "Chu đáo", "Có trách nhiệm", "Chú ý chi tiết" },
                    Compatible = new List<string> { "Taurus", "Capricorn", "Cancer", "Scorpio" },
                    LessCompatible = new List<string> { "Gemini", "Sagittarius" },
                    OppositSign = "Pisces",
                    SymbolMeaning = "Người trinh nữ cầm bó lúa tượng trưng cho sự tinh khiết và trí tuệ",
                    LuckyColor = "Xanh lá, Nâu",
                    LuckyNumber = "5, 14",
                    LuckyDay = "Thứ Tư"
                },
                new ZodiacSign
                {
                    Id = 7,
                    Name = "Thiên Bình",
                    EnglishName = "Libra",
                    Symbol = "♎",
                    IconUrl = "/images/zodiac/libra.svg",
                    DateRange = "23/9 – 23/10",
                    StartMonth = 9, StartDay = 23,
                    EndMonth = 10, EndDay = 23,
                    Element = ZodiacElement.Air,
                    Modality = ZodiacModality.Cardinal,
                    RulingPlanet = "Venus",
                    RulingPlanetVietnamese = "Sao Kim",
                    Description = "Thiên Bình công bằng, hòa giải, coi trọng sự hòa hợp và có dáng dấp lịch sự tao nhã.",
                    Strengths = new List<string> { "Cân bằng", "Công bằng", "Duyên dáng", "Khoan dung", "Giao tiếp tốt" },
                    Weaknesses = new List<string> { "Thiếu quyết đoán", "Do dự", "Khó phân biệt thiện ác", "Tự cao" },
                    Traits = new List<string> { "Công bằng", "Hòa giải", "Coi trọng hòa hợp", "Lịch sự" },
                    Compatible = new List<string> { "Gemini", "Aquarius", "Leo", "Sagittarius" },
                    LessCompatible = new List<string> { "Cancer", "Capricorn" },
                    OppositSign = "Aries",
                    SymbolMeaning = "Cán cân tượng trưng cho công lý và sự cân bằng",
                    LuckyColor = "Hồng, Xanh lá",
                    LuckyNumber = "4, 6, 9",
                    LuckyDay = "Thứ Sáu"
                },
                new ZodiacSign
                {
                    Id = 8,
                    Name = "Thiên Yết",
                    EnglishName = "Scorpio",
                    Symbol = "♏",
                    IconUrl = "/images/zodiac/scorpio.svg",
                    DateRange = "24/10 – 22/11",
                    StartMonth = 10, StartDay = 24,
                    EndMonth = 11, EndDay = 22,
                    Element = ZodiacElement.Water,
                    Modality = ZodiacModality.Fixed,
                    RulingPlanet = "Pluto",
                    RulingPlanetVietnamese = "Sao Diêm Vương",
                    Description = "Thiên Yết mạnh mẽ, bí ẩn, giàu nghị lực và đam mê.",
                    Strengths = new List<string> { "Quyết đoán", "Kiên cường", "Trung thành", "Trực giác", "Đam mê" },
                    Weaknesses = new List<string> { "Đố kỵ", "Chiếm hữu", "Bướng bỉnh", "Khó tha thứ", "Bí ẩn" },
                    Traits = new List<string> { "Mạnh mẽ", "Bí ẩn", "Nghị lực", "Đam mê" },
                    Compatible = new List<string> { "Cancer", "Pisces", "Virgo", "Capricorn" },
                    LessCompatible = new List<string> { "Leo", "Aquarius" },
                    OppositSign = "Taurus",
                    SymbolMeaning = "Con bọ cạp với nọc độc tượng trưng cho khả năng tự vệ mạnh mẽ",
                    LuckyColor = "Đỏ sẫm, Đen",
                    LuckyNumber = "8, 11",
                    LuckyDay = "Thứ Ba"
                },
                new ZodiacSign
                {
                    Id = 9,
                    Name = "Nhân Mã",
                    EnglishName = "Sagittarius",
                    Symbol = "♐",
                    IconUrl = "/images/zodiac/sagittarius.svg",
                    DateRange = "23/11 – 21/12",
                    StartMonth = 11, StartDay = 23,
                    EndMonth = 12, EndDay = 21,
                    Element = ZodiacElement.Fire,
                    Modality = ZodiacModality.Mutable,
                    RulingPlanet = "Jupiter",
                    RulingPlanetVietnamese = "Sao Mộc",
                    Description = "Nhân Mã yêu tự do, lạc quan, thích khám phá và thẳng thắn.",
                    Strengths = new List<string> { "Hài hước", "Lạc quan", "Trung thực", "Nhiệt tình", "Phiêu lưu" },
                    Weaknesses = new List<string> { "Bốc đồng", "Thiếu kiên nhẫn", "Vô trách nhiệm", "Thiếu chiều sâu" },
                    Traits = new List<string> { "Yêu tự do", "Lạc quan", "Khám phá", "Thẳng thắn" },
                    Compatible = new List<string> { "Aries", "Leo", "Libra", "Aquarius" },
                    LessCompatible = new List<string> { "Virgo", "Pisces" },
                    OppositSign = "Gemini",
                    SymbolMeaning = "Người cung thủ tượng trưng cho trí tuệ và khát khao khám phá",
                    LuckyColor = "Tím, Xanh dương",
                    LuckyNumber = "3, 7, 9",
                    LuckyDay = "Thứ Năm"
                },
                new ZodiacSign
                {
                    Id = 10,
                    Name = "Ma Kết",
                    EnglishName = "Capricorn",
                    Symbol = "♑",
                    IconUrl = "/images/zodiac/capricorn.svg",
                    DateRange = "22/12 – 19/1",
                    StartMonth = 12, StartDay = 22,
                    EndMonth = 1, EndDay = 19,
                    Element = ZodiacElement.Earth,
                    Modality = ZodiacModality.Cardinal,
                    RulingPlanet = "Saturn",
                    RulingPlanetVietnamese = "Sao Thổ",
                    Description = "Ma Kết tham vọng, có trách nhiệm, thực tế và luôn tuân theo nguyên tắc.",
                    Strengths = new List<string> { "Kiên nhẫn", "Kỷ luật", "Trách nhiệm", "Quyết tâm", "Tổ chức tốt" },
                    Weaknesses = new List<string> { "Cứng nhắc", "Bi quan", "Làm việc quá sức", "Lạnh lùng" },
                    Traits = new List<string> { "Tham vọng", "Trách nhiệm", "Thực tế", "Tuân thủ nguyên tắc" },
                    Compatible = new List<string> { "Taurus", "Virgo", "Scorpio", "Pisces" },
                    LessCompatible = new List<string> { "Aries", "Libra" },
                    OppositSign = "Cancer",
                    SymbolMeaning = "Dê biển tượng trưng cho khả năng vượt qua địa hình khó khăn",
                    LuckyColor = "Nâu, Đen",
                    LuckyNumber = "4, 8, 13, 22",
                    LuckyDay = "Thứ Bảy"
                },
                new ZodiacSign
                {
                    Id = 11,
                    Name = "Bảo Bình",
                    EnglishName = "Aquarius",
                    Symbol = "♒",
                    IconUrl = "/images/zodiac/aquarius.svg",
                    DateRange = "20/1 – 18/2",
                    StartMonth = 1, StartDay = 20,
                    EndMonth = 2, EndDay = 18,
                    Element = ZodiacElement.Air,
                    Modality = ZodiacModality.Fixed,
                    RulingPlanet = "Uranus",
                    RulingPlanetVietnamese = "Sao Thiên Vương",
                    Description = "Bảo Bình độc lập, sáng tạo, thông minh, nhân đạo và thích sự mới mẻ.",
                    Strengths = new List<string> { "Thông minh", "Độc đáo", "Trung thực", "Tầm nhìn xa", "Sáng tạo" },
                    Weaknesses = new List<string> { "Lạnh lùng", "Khó đoán", "Bướng bỉnh", "Xa cách", "Quá lý trí" },
                    Traits = new List<string> { "Độc lập", "Sáng tạo", "Thông minh", "Nhân đạo", "Mới mẻ" },
                    Compatible = new List<string> { "Gemini", "Libra", "Aries", "Sagittarius" },
                    LessCompatible = new List<string> { "Taurus", "Scorpio" },
                    OppositSign = "Leo",
                    SymbolMeaning = "Người mang bình nước tượng trưng cho sự ban phát tri thức",
                    LuckyColor = "Xanh dương, Bạc",
                    LuckyNumber = "4, 7, 11, 22",
                    LuckyDay = "Thứ Bảy"
                },
                new ZodiacSign
                {
                    Id = 12,
                    Name = "Song Ngư",
                    EnglishName = "Pisces",
                    Symbol = "♓",
                    IconUrl = "/images/zodiac/pisces.svg",
                    DateRange = "19/2 – 20/3",
                    StartMonth = 2, StartDay = 19,
                    EndMonth = 3, EndDay = 20,
                    Element = ZodiacElement.Water,
                    Modality = ZodiacModality.Mutable,
                    RulingPlanet = "Neptune",
                    RulingPlanetVietnamese = "Sao Hải Vương",
                    Description = "Song Ngư nhạy cảm, mơ mộng, giàu lòng trắc ẩn và trực giác tốt.",
                    Strengths = new List<string> { "Đồng cảm", "Vị tha", "Sáng tạo", "Trực giác", "Từ bi" },
                    Weaknesses = new List<string> { "Dễ bị ảnh hưởng", "Trốn tránh thực tế", "Thiếu quyết đoán", "Yếu đuối" },
                    Traits = new List<string> { "Nhạy cảm", "Mơ mộng", "Trắc ẩn", "Trực giác tốt" },
                    Compatible = new List<string> { "Cancer", "Scorpio", "Taurus", "Capricorn" },
                    LessCompatible = new List<string> { "Gemini", "Sagittarius" },
                    OppositSign = "Virgo",
                    SymbolMeaning = "Hai con cá bơi ngược chiều tượng trưng cho tính hai mặt",
                    LuckyColor = "Xanh lá cây, Tím",
                    LuckyNumber = "3, 9, 12",
                    LuckyDay = "Thứ Năm"
                }
            });
        }

        private async Task<DailyHoroscope> GenerateDailyHoroscopeAsync(int zodiacSignId, DateTime date)
        {
            // In a real application, this would fetch from an external API or database
            // For now, we'll generate sample data
            var sign = await GetZodiacSignByIdAsync(zodiacSignId);
            if (sign == null) return null!;

            var random = new Random(date.Day + zodiacSignId);
            
            return new DailyHoroscope
            {
                Id = zodiacSignId * 1000 + date.DayOfYear,
                ZodiacSignId = zodiacSignId,
                ZodiacSign = sign,
                Date = date.Date,
                Content = GenerateHoroscopeContent(sign.Name, date),
                LuckyNumber = $"{random.Next(1, 50)}",
                LuckyColor = GetRandomColor(random),
                Mood = GetRandomMood(random),
                LoveScore = random.Next(60, 100),
                CareerScore = random.Next(60, 100),
                HealthScore = random.Next(60, 100),
                MoneyScore = random.Next(60, 100),
                CreatedAt = DateTime.UtcNow
            };
        }

        private string GenerateHoroscopeContent(string signName, DateTime date)
        {
            var templates = new List<string>
            {
                $"Hôm nay là một ngày tuyệt vời dành cho {signName}! Hãy tận dụng năng lượng tích cực để thực hiện những kế hoạch quan trọng.",
                $"Cung {signName} có thể gặp một số thử thách nhỏ hôm nay, nhưng với sự kiên nhẫn, bạn sẽ vượt qua dễ dàng.",
                $"Đây là thời điểm lý tưởng để {signName} tập trung vào phát triển bản thân và mối quan hệ xung quanh.",
                $"Những cơ hội mới đang chờ đợi {signName}. Hãy mở lòng và đón nhận chúng!",
                $"{signName} nên chú ý đến sức khỏe và cân bằng giữa công việc và cuộc sống cá nhân ngày hôm nay."
            };
            
            var random = new Random(date.DayOfYear);
            return templates[random.Next(templates.Count)];
        }

        private string GetRandomColor(Random random)
        {
            var colors = new[] { "Đỏ", "Xanh dương", "Vàng", "Xanh lá", "Tím", "Cam", "Hồng", "Trắng" };
            return colors[random.Next(colors.Length)];
        }

        private string GetRandomMood(Random random)
        {
            var moods = new[] { "Vui vẻ", "Năng động", "Bình tĩnh", "Sáng tạo", "Tự tin", "Lạc quan" };
            return moods[random.Next(moods.Length)];
        }

        #endregion
    }
}
