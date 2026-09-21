using Microsoft.AspNetCore.Http;
using UnistayWeb.DAL.Models.Room;
using UnistayWeb.DAL.Models.User;

namespace UnistayWeb.BLL.ViewModels
{
    public class CreateRoomDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Deposit { get; set; }
        public decimal Area { get; set; }
        public string Address { get; set; } = string.Empty;
        public int MaxOccupants { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Rules { get; set; }
        public List<string>? SelectedAmenities { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }

    public class RoomDetailDto
    {
        public Room Room { get; set; } = null!;
        public List<RoomImage> Images { get; set; } = new();
        public UserProfile? Owner { get; set; }
    }

    public class RoomListItemDto
    {
        public Room Room { get; set; } = null!;
        public List<RoomImage> Images { get; set; } = new();
        public UserProfile? Owner { get; set; }
    }

    public class RoomOperationResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Room? Room { get; set; }
    }
}
