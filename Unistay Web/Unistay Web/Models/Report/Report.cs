using System.ComponentModel.DataAnnotations;

namespace Unistay_Web.Models.Report
{
    public class Report
    {
        public int Id { get; set; }
        
        [Required]
        public string ReporterId { get; set; } = string.Empty;
        
        [Required]
        public string ReportedEntityType { get; set; } = string.Empty;
        
        [Required]
        public int ReportedEntityId { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
    }
}
