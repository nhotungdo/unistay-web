using System.ComponentModel.DataAnnotations;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.DAL.Models.Connection;

namespace UnistayWeb.BLL.ViewModels
{
    public class EditProfileDto
    {
        [Required(ErrorMessage = "Tên đầy đủ không được để trống.")]
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Occupation { get; set; }
        public string? Bio { get; set; }
        public string? LivingArea { get; set; }
        public decimal? Budget { get; set; }
        public string? Lifestyle { get; set; }
        public DateTime? ExpectedStayDuration { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class NotificationPreferencesDto
    {
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public bool PushEnabled { get; set; }
    }

    public class ProfileDto
    {
        public UserProfile UserProfile { get; set; } = null!;
        public List<ActivityHistory> ActivityHistory { get; set; } = new();
    }

    public class PagedHistoryDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class PublicProfileDto
    {
        public UserProfile UserProfile { get; set; } = null!;
        public List<ActivityHistory> Activities { get; set; } = new();
        public string ConnectionStatus { get; set; } = "None";
        public int ConnectionId { get; set; }
    }
}
