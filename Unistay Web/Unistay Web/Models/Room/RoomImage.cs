namespace Unistay_Web.Models.Room
{
    public class RoomImage
    {
        public int Id { get; set; }
        
        public int RoomId { get; set; }
        
        public string ImageUrl { get; set; } = string.Empty;
        
        public bool IsPrimary { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
