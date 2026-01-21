using System;

namespace Unistay_Web.Helpers
{
    public class ZodiacInfo
    {
        public string Name { get; set; } = "";
        public string VietnameseName { get; set; } = "";
        public string Icon { get; set; } = "";
        public string DateRange { get; set; } = "";
        public string Traits { get; set; } = "";
        public string Compatibility { get; set; } = "";
        public string Element { get; set; } = "";
    }

    public static class ZodiacHelper
    {
        public static ZodiacInfo GetZodiacInfo(DateTime dateOfBirth)
        {
            int day = dateOfBirth.Day;
            int month = dateOfBirth.Month;
            string sign = "";

            if ((month == 3 && day >= 21) || (month == 4 && day <= 19)) sign = "Aries";
            else if ((month == 4 && day >= 20) || (month == 5 && day <= 20)) sign = "Taurus";
            else if ((month == 5 && day >= 21) || (month == 6 && day <= 20)) sign = "Gemini";
            else if ((month == 6 && day >= 21) || (month == 7 && day <= 22)) sign = "Cancer";
            else if ((month == 7 && day >= 23) || (month == 8 && day <= 22)) sign = "Leo";
            else if ((month == 8 && day >= 23) || (month == 9 && day <= 22)) sign = "Virgo";
            else if ((month == 9 && day >= 23) || (month == 10 && day <= 22)) sign = "Libra";
            else if ((month == 10 && day >= 23) || (month == 11 && day <= 21)) sign = "Scorpio";
            else if ((month == 11 && day >= 22) || (month == 12 && day <= 21)) sign = "Sagittarius";
            else if ((month == 12 && day >= 22) || (month == 1 && day <= 19)) sign = "Capricorn";
            else if ((month == 1 && day >= 20) || (month == 2 && day <= 18)) sign = "Aquarius";
            else sign = "Pisces";

            return GetZodiacInfoByName(sign);
        }

        public static ZodiacInfo GetZodiacInfoByName(string signName)
        {
            // Normalize
            var key = signName?.Trim().ToLower();
            
            return key switch
            {
                "aries" => new ZodiacInfo
                {
                    Name = "Aries",
                    VietnameseName = "Bạch Dương",
                    Icon = "♈",
                    DateRange = "21/03 - 19/04",
                    Traits = "Tiên phong, dũng cảm, tự tin, năng động",
                    Compatibility = "Leo, Sagittarius",
                    Element = "Fire"
                },
                "taurus" => new ZodiacInfo
                {
                    Name = "Taurus",
                    VietnameseName = "Kim Ngưu",
                    Icon = "♉",
                    DateRange = "20/04 - 20/05",
                    Traits = "Kiên định, đáng tin cậy, thực tế, tận tâm",
                    Compatibility = "Virgo, Capricorn",
                    Element = "Earth"
                },
                "gemini" => new ZodiacInfo
                {
                    Name = "Gemini",
                    VietnameseName = "Song Tử",
                    Icon = "♊",
                    DateRange = "21/05 - 20/06",
                    Traits = "Linh hoạt, tò mò, giao tiếp tốt, hóm hỉnh",
                    Compatibility = "Libra, Aquarius",
                    Element = "Air"
                },
                "cancer" => new ZodiacInfo
                {
                    Name = "Cancer",
                    VietnameseName = "Cự Giải",
                    Icon = "♋",
                    DateRange = "21/06 - 22/07",
                    Traits = "Nhạy cảm, chu đáo, trung thành, giàu trí tưởng tượng",
                    Compatibility = "Scorpio, Pisces",
                    Element = "Water"
                },
                "leo" => new ZodiacInfo
                {
                    Name = "Leo",
                    VietnameseName = "Sư Tử",
                    Icon = "♌",
                    DateRange = "23/07 - 22/08",
                    Traits = "Sáng tạo, nhiệt huyết, hào phóng, lãnh đạo",
                    Compatibility = "Aries, Sagittarius",
                    Element = "Fire"
                },
                "virgo" => new ZodiacInfo
                {
                    Name = "Virgo",
                    VietnameseName = "Xử Nữ",
                    Icon = "♍",
                    DateRange = "23/08 - 22/09",
                    Traits = "Tỉ mỉ, phân tích tốt, thực tế, chăm chỉ",
                    Compatibility = "Taurus, Capricorn",
                    Element = "Earth"
                },
                "libra" => new ZodiacInfo
                {
                    Name = "Libra",
                    VietnameseName = "Thiên Bình",
                    Icon = "♎",
                    DateRange = "23/09 - 22/10",
                    Traits = "Công bằng, hòa nhã, ngoại giao, duy mỹ",
                    Compatibility = "Gemini, Aquarius",
                    Element = "Air"
                },
                "scorpio" => new ZodiacInfo
                {
                    Name = "Scorpio",
                    VietnameseName = "Thiên Yết",
                    Icon = "♏",
                    DateRange = "23/10 - 21/11",
                    Traits = "Mạnh mẽ, bí ẩn, quyết đoán, sâu sắc",
                    Compatibility = "Cancer, Pisces",
                    Element = "Water"
                },
                "sagittarius" => new ZodiacInfo
                {
                    Name = "Sagittarius",
                    VietnameseName = "Nhân Mã",
                    Icon = "♐",
                    DateRange = "22/11 - 21/12",
                    Traits = "Lạc quan, yêu tự do, hài hước, triết lý",
                    Compatibility = "Aries, Leo",
                    Element = "Fire"
                },
                "capricorn" => new ZodiacInfo
                {
                    Name = "Capricorn",
                    VietnameseName = "Ma Kết",
                    Icon = "♑",
                    DateRange = "22/12 - 19/01",
                    Traits = "Kỷ luật, có trách nhiệm, tham vọng, kiên nhẫn",
                    Compatibility = "Taurus, Virgo",
                    Element = "Earth"
                },
                "aquarius" => new ZodiacInfo
                {
                    Name = "Aquarius",
                    VietnameseName = "Bảo Bình",
                    Icon = "♒",
                    DateRange = "20/01 - 18/02",
                    Traits = "Độc đáo, sáng tạo, nhân ái, độc lập",
                    Compatibility = "Gemini, Libra",
                    Element = "Air"
                },
                "pisces" => new ZodiacInfo
                {
                    Name = "Pisces",
                    VietnameseName = "Song Ngư",
                    Icon = "♓",
                    DateRange = "19/02 - 20/03",
                    Traits = "Mơ mộng, nghệ sĩ, trực giác tốt, từ bi",
                    Compatibility = "Cancer, Scorpio",
                    Element = "Water"
                },
                _ => new ZodiacInfo // Default/Unknown
                {
                    Name = "Unknown",
                    VietnameseName = "Chưa rõ",
                    Icon = "❓",
                    DateRange = "",
                    Traits = "",
                    Compatibility = "",
                    Element = ""
                }
            };
        }
    }
}
