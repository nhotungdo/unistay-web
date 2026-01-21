using Unistay_Web.Models.Roommate;

namespace Unistay_Web.Services
{
    public interface IAiMatchingService
    {
        Task<AiAnalysisResult> AnalyzeCompatibilityAsync(string userId1, string userId2, string priority = "balanced");
    }
}
