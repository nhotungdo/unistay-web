using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.Roommate;

namespace Unistay_Web.Services
{
    public class AiMatchingService : IAiMatchingService
    {
        private readonly ApplicationDbContext _context;

        public AiMatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AiAnalysisResult> AnalyzeCompatibilityAsync(string userId1, string userId2, string priority = "balanced")
        {
            var user1 = await _context.Users.FindAsync(userId1);
            var user2 = await _context.Users.FindAsync(userId2);
            var profile1 = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == userId1);
            var profile2 = await _context.RoommateProfiles.FirstOrDefaultAsync(p => p.UserId == userId2);

            if (user1 == null || user2 == null) return new AiAnalysisResult();

            var result = new AiAnalysisResult();
            var rand = new Random(userId1.GetHashCode() + userId2.GetHashCode());

            // 1. Calculate Component Scores
            // Budget Compatibility
            int budgetScore = 100;
            if (profile1 != null && profile2 != null)
            {
                 decimal diff = Math.Abs(profile1.Budget - profile2.Budget);
                 if (diff > 2000000) budgetScore = 50;
                 else if (diff > 1000000) budgetScore = 70;
                 else budgetScore = 95;
            }
            result.ComponentScores.Add("Ngân sách", budgetScore);

            // Lifestyle Compatibility
            int lifestyleScore = rand.Next(60, 100);
            result.ComponentScores.Add("Lối sống", lifestyleScore);

            // Personality
            int personalityScore = rand.Next(50, 95);
            result.ComponentScores.Add("Tính cách", personalityScore);

             // Location
            int locationScore = 60;
            if (profile1?.PreferredArea != null && profile2?.PreferredArea != null && 
                profile1.PreferredArea.Contains(profile2.PreferredArea, StringComparison.OrdinalIgnoreCase))
            {
                locationScore = 90;
            }
            else
            {
                 locationScore = rand.Next(60, 90);
            }
            result.ComponentScores.Add("Khu vực", locationScore);

            // Overall - Adjust weights based on priority
            double wBudget = 0.4, wLifestyle = 0.3, wPersonality = 0.2, wLocation = 0.1;
            
            switch (priority.ToLower())
            {
                case "budget":
                    wBudget = 0.6; wLifestyle = 0.2; wPersonality = 0.1; wLocation = 0.1;
                    break;
                case "lifestyle":
                    wBudget = 0.2; wLifestyle = 0.5; wPersonality = 0.2; wLocation = 0.1;
                    break;
                case "personality":
                    wBudget = 0.2; wLifestyle = 0.2; wPersonality = 0.5; wLocation = 0.1;
                    break;
            }

            result.OverallScore = (int)((budgetScore * wBudget) + (lifestyleScore * wLifestyle) + (personalityScore * wPersonality) + (locationScore * wLocation));

            // 2. Generate Report
            result.AnalysisReport = GenerateReport(result.OverallScore, user2.FullName);

            // 3. Shared Interests (Mock from Habits)
            if (profile1?.Habits != null && profile2?.Habits != null)
            {
                var habits1 = profile1.Habits.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim());
                var habits2 = profile2.Habits.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim());
                result.SharedInterests = habits1.Intersect(habits2).ToList();
            }
            if (!result.SharedInterests.Any())
            {
                result.SharedInterests = new List<string> { "Xem phim", "Du lịch" }; // Default fallbacks
            }

            // 4. Potential Conflicts
            if (budgetScore < 70) result.PotentialConflicts.Add("Chênh lệch ngân sách");
            if (lifestyleScore < 70) result.PotentialConflicts.Add("Giờ giấc sinh hoạt khác biệt");
            if (result.PotentialConflicts.Count == 0) result.PotentialConflicts.Add("Chưa phát hiện xung đột đáng kể");

            // Mock delay to simulate AI processing if needed, but requirements say < 3s.
            // EF Core calls are fast enough.

            return result;
        }

        private string GenerateReport(int score, string name)
        {
            if (score >= 90) return $"Rất tuyệt vời! Bạn và {name} là một cặp đôi hoàn hảo để ở ghép. Hai bạn có sự đồng điệu cao về cả ngân sách và lối sống.";
            if (score >= 70) return $"Khá tốt. Bạn và {name} có nhiều điểm chung, đặc biệt là về ngân sách. Tuy nhiên, hãy trao đổi thêm về thói quen sinh hoạt để hiểu nhau hơn.";
            if (score >= 50) return $"Trung bình. Có một số khác biệt giữa bạn và {name}. Cần cân nhắc kỹ và thống nhất các quy tắc trước khi quyết định ở ghép.";
            return $"Cần xem xét kỹ. Mức độ phù hợp giữa bạn và {name} chưa cao, có thể sẽ gặp khó khăn trong việc dung hòa lối sống.";
        }
    }
}
