using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;
using Unistay_Web.Models.RentalAdvice;
using Unistay_Web.Models.Room;

namespace Unistay_Web.Services.RentalAdvice
{
    public class RentalAdviceService : IRentalAdviceService
    {
        private readonly ApplicationDbContext _context;

        public RentalAdviceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetPersonalizedAdviceAsync(string userId)
        {
            // Simulate AI analysis
            await Task.Delay(500); // Simulate processing
            return "Based on your activity, we recommend considering properties in the downtown area which offer the best commute times. You seem to prefer modern apartments with high walkability scores.";
        }

        public async Task<string> GetChatResponseAsync(string message, string userId)
        {
            await Task.Delay(500); 
            if (message.ToLower().Contains("price"))
                return "Rental prices in this area are trending upwards. I suggest locking in a lease soon.";
            if (message.ToLower().Contains("safe"))
                return "The neighborhoods you are looking at have a high safety rating, though always verify specific streets.";
            return "That's a great question. When looking for a rental, consider the proximity to public transport and local amenities.";
        }

        public async Task<List<NeighborhoodDetail>> GetNeighborhoodSuggestionsAsync(string userId)
        {
            // specific data simulation
            return new List<NeighborhoodDetail>
            {
                new NeighborhoodDetail { Name = "Tech Hub District", Description = "Vibrant area with many startups.", AveragePrice = 1200, SafetyScore = 8.5, WalkabilityScore = 9, Amenities = new List<string> { "Gyms", "Cafes", "Metro" }, CommuteTimeMinutes = 15, RiskLevel = "Low" },
                new NeighborhoodDetail { Name = "Green Leaf Park", Description = "Quiet and family friendly.", AveragePrice = 950, SafetyScore = 9.2, WalkabilityScore = 7, Amenities = new List<string> { "Parks", "Schools" }, CommuteTimeMinutes = 35, RiskLevel = "Low" },
                new NeighborhoodDetail { Name = "Industrial Lofts", Description = "Up and coming area.", AveragePrice = 800, SafetyScore = 6.5, WalkabilityScore = 0, Amenities = new List<string> { "Art Galleries", "Bars" }, CommuteTimeMinutes = 25, RiskLevel = "Medium" }
            };
        }

        public async Task<List<Room>> GetRoomSuggestionsAsync(string userId)
        {
            // Return random 3 rooms for now
            return await _context.Rooms.Take(3).ToListAsync();
        }

        public async Task<RiskAnalysis> AnalyzeRoomRisksAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            var analysis = new RiskAnalysis
            {
                RoomId = roomId,
                RiskScore = 15,
                LandlordRating = 4.5,
                IsVerified = true,
                RiskFactors = new List<string>(),
                SafetyWarnings = new List<string>(),
                FraudIndicators = new List<string>()
            };

            if (room != null)
            {
                if (room.Price < 500) // Suspiciously low
                {
                    analysis.RiskScore += 40;
                    analysis.FraudIndicators.Add("Price is significantly below market average.");
                    analysis.RiskFactors.Add("Potential scam listing.");
                }
                if (string.IsNullOrEmpty(room.Description) || room.Description.Length < 50)
                {
                    analysis.RiskScore += 10;
                    analysis.RiskFactors.Add("Listing lacks detailed description.");
                }
            }
            return analysis;
        }

        public async Task<bool> DetectFraudAsync(int roomId)
        {
            var analysis = await AnalyzeRoomRisksAsync(roomId);
            return analysis.RiskScore > 70;
        }

        public async Task<PriceTrend> GetPriceTrendsAsync(string area)
        {
            return new PriceTrend
            {
                AreaName = area,
                BestTimeToRent = "September",
                MarketStatus = "Hot",
                ProjectedPriceNextMonth = 1050,
                MonthlyAverages = new Dictionary<string, decimal>
                {
                    { "Jan", 950 }, { "Feb", 960 }, { "Mar", 980 },
                    { "Apr", 1000 }, { "May", 1020 }, { "Jun", 1100 }
                }
            };
        }

        public async Task<string> GetBestTimeToRentAsync(string area)
        {
            return "The best time to rent in " + area + " is usually during the winter months when demand is lower.";
        }
    }
}
