namespace Unistay_Web.Models.Roommate
{
    public class RoommateDisplayDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = "Người dùng";
        public string? AvatarUrl { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public decimal Budget { get; set; }
        public string? PreferredArea { get; set; }
        public string[] Habits { get; set; } = Array.Empty<string>();
        public int MatchPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
