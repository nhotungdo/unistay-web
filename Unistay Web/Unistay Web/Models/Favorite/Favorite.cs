using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Favorite
{
    public class Favorite
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int RoomId { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
