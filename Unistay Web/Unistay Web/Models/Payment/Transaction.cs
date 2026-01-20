using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Payment
{
    public class Transaction
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        public string Type { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Pending";
        
        public string? Description { get; set; }
        
        public string? PaymentMethod { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
    }
}
