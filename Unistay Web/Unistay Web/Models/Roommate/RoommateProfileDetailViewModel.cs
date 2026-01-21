using System.Collections.Generic;

namespace Unistay_Web.Models.Roommate
{
    public class RoommateProfileDetailViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? Bio { get; set; }
        
        // Roommate Specifics
        public decimal Budget { get; set; }
        public string? PreferredArea { get; set; }
        public DateTime? MoveInDate { get; set; }
        public List<string> Habits { get; set; } = new List<string>();
        
        // AI Analysis Data
        public int CompatibilityScore { get; set; } // 0-100
        public Dictionary<string, int> CompatibilityBreakdown { get; set; } = new Dictionary<string, int>();
        public string AiAnalysisReport { get; set; } // Text summary
        public List<string> SharedInterests { get; set; } = new List<string>();
        public List<string> PotentialConflicts { get; set; } = new List<string>();
    }
}
