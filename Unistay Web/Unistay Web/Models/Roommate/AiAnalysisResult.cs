using System.Collections.Generic;

namespace Unistay_Web.Models.Roommate
{
    public class AiAnalysisResult
    {
        public int OverallScore { get; set; }
        public Dictionary<string, int> ComponentScores { get; set; } = new Dictionary<string, int>();
        public string AnalysisReport { get; set; } = string.Empty;
        public List<string> SharedInterests { get; set; } = new List<string>();
        public List<string> PotentialConflicts { get; set; } = new List<string>();
        public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    }
}
