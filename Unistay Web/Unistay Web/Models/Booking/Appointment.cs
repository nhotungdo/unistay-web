using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Booking
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        public string? Notes { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
