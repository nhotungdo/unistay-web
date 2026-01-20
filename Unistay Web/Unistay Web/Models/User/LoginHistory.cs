using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unistay_Web.Models.User
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public UserProfile? User { get; set; }

        [Required]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        public DateTime? LogoutTime { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; } // Browser/Device info

        [StringLength(100)]
        public string? Browser { get; set; }

        [StringLength(100)]
        public string? OperatingSystem { get; set; }

        [StringLength(100)]
        public string? DeviceType { get; set; } // Desktop, Mobile, Tablet

        public bool IsSuccessful { get; set; } = true;

        [StringLength(500)]
        public string? FailureReason { get; set; }

        public string? AuthenticationMethod { get; set; } // "Password", "Google", "Facebook"

        public TimeSpan? SessionDuration => LogoutTime.HasValue ? LogoutTime.Value - LoginTime : null;
    }
}
