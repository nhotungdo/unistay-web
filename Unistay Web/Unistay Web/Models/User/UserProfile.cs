using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Unistay_Web.Models.User
{
    public class UserProfile : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        public int? Age { get; set; }

        public string? Gender { get; set; }

        public string? Occupation { get; set; }

        public string? LivingArea { get; set; }

        public decimal? Budget { get; set; }

        public string? Lifestyle { get; set; }

        public DateTime? ExpectedStayDuration { get; set; }

        public bool IsVerified { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Provider { get; set; } // "Local", "Google", "Facebook"

        public string? ProviderKey { get; set; }

        // New fields for enhanced profile management
        [StringLength(500)]
        public string? Bio { get; set; }

        public string? IdCardNumber { get; set; } // CCCD/ID number

        public bool IsIdVerified { get; set; } // Identity verification status

        public DateTime? IdVerificationDate { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Ward { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Account status
        public bool IsActive { get; set; } = true;

        public bool IsBlocked { get; set; } = false;

        public string? BlockReason { get; set; }

        public DateTime? BlockedDate { get; set; }

        // Verification tracking
        public DateTime? EmailVerifiedDate { get; set; }

        public DateTime? PhoneVerifiedDate { get; set; }

        // Profile stats
        public int TotalListings { get; set; } = 0;

        public decimal AverageRating { get; set; } = 0;

        public int TotalReviews { get; set; } = 0;

        public int SuccessfulBookings { get; set; } = 0;

        // Preferences
        public bool NotificationEmailEnabled { get; set; } = true;

        public bool NotificationSmsEnabled { get; set; } = true;

        public bool NotificationPushEnabled { get; set; } = true;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public string? LastLoginIp { get; set; }
    }
}
