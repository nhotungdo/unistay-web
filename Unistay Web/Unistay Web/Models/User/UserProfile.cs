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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
