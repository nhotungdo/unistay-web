using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unistay_Web.Models.User;

namespace Unistay_Web.Models.Connection
{
    public enum ConnectionStatus
    {
        Pending,
        Accepted,
        Declined
    }

    public class Connection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; }

        [ForeignKey("RequesterId")]
        public virtual UserProfile Requester { get; set; } = null!;

        [Required]
        public string AddresseeId { get; set; }

        [ForeignKey("AddresseeId")]
        public virtual UserProfile Addressee { get; set; } = null!;

        [Required]
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
