using System;
using System.Collections.Generic;

namespace Unistay_Web.Models.RentalAdvice
{
    public class RiskAnalysis
    {
        public int RoomId { get; set; }
        public double RiskScore { get; set; } // 0-100, higher is riskier
        public List<string> RiskFactors { get; set; } = new List<string>();
        public List<string> SafetyWarnings { get; set; } = new List<string>();
        public double LandlordRating { get; set; }
        public bool IsVerified { get; set; }
        public List<string> FraudIndicators { get; set; } = new List<string>();
    }

    public class NeighborhoodDetail
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double AveragePrice { get; set; }
        public double SafetyScore { get; set; } // 0-10
        public double WalkabilityScore { get; set; } // 0-10
        public List<string> Amenities { get; set; } = new List<string>();
        public double CommuteTimeMinutes { get; set; }
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
    }

    public class PriceTrend
    {
        public string AreaName { get; set; } = string.Empty;
        public Dictionary<string, decimal> MonthlyAverages { get; set; } = new Dictionary<string, decimal>();
        public string BestTimeToRent { get; set; } = string.Empty;
        public decimal ProjectedPriceNextMonth { get; set; }
        public string MarketStatus { get; set; } = "Stable"; // Hot, Cold, Stable
    }
}
