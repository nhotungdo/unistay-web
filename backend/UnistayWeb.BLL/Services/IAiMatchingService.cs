using UnistayWeb.DAL.Models.Roommate;

namespace UnistayWeb.BLL.Services
{
    public interface IAiMatchingService
    {
        Task<AiAnalysisResult> AnalyzeCompatibilityAsync(string userId1, string userId2, string priority = "balanced");
    }
}
