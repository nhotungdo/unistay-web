using System.Collections.Generic;
using System.Threading.Tasks;
using UnistayWeb.DAL.Models.RentalAdvice;
using UnistayWeb.DAL.Models.Room;

namespace UnistayWeb.BLL.Services.RentalAdvice
{
    public interface IRentalAdviceService
    {
        Task<string> GetPersonalizedAdviceAsync(string userId);
        Task<string> GetChatResponseAsync(string message, string userId);
        Task<List<NeighborhoodDetail>> GetNeighborhoodSuggestionsAsync(string userId);
        Task<List<Room>> GetRoomSuggestionsAsync(string userId);
        Task<RiskAnalysis> AnalyzeRoomRisksAsync(int roomId);
        Task<bool> DetectFraudAsync(int roomId);
        Task<PriceTrend> GetPriceTrendsAsync(string area);
        Task<string> GetBestTimeToRentAsync(string area);
    }
}
