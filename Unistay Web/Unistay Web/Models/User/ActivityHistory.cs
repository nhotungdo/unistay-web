using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Unistay_Web.Models.User
{
    public class ActivityHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public UserProfile? User { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty;
        // Types: ProfileUpdate, ListingCreated, ListingUpdated, ListingDeleted, 
        //        BookingCreated, ReviewSubmitted, ChatInitiated, AvatarChanged

        [Required]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? RelatedEntity { get; set; } // Room ID, Listing ID, etc.

        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // Room, Booking, Review, etc.

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public bool IsPublic { get; set; } = true; // Whether to show in public activity
    }
}
