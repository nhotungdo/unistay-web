using Microsoft.AspNetCore.Http;

namespace UnistayWeb.BLL.ViewModels
{
    public class CreateRoommateDto
    {
        public decimal Budget { get; set; }
        public string? PreferredArea { get; set; }
        public DateTime? MoveInDate { get; set; }
        public string? Habits { get; set; }
        public string? Gender { get; set; }
        public string? FullName { get; set; }
        public int? BirthYear { get; set; }
        public string? Occupation { get; set; }
        public string? Introduction { get; set; }
        public IFormFile? AvatarUpload { get; set; }
    }
}
