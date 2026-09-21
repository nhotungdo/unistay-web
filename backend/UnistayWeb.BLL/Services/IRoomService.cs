using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Models.Room;

namespace UnistayWeb.BLL.Services
{
    public interface IRoomService
    {
        Task<List<RoomListItemDto>> GetRoomsAsync(string? location, string? price);
        Task<RoomDetailDto?> GetRoomAsync(int id);
        Task<Room> CreateRoomAsync(CreateRoomDto dto, string userId, string webRootPath);
        Task<bool> EditRoomAsync(int id, Room room, string userId);
        Task<bool> DeleteRoomAsync(int id, string userId);
        bool RoomExists(int id);
        Task<List<Room>> GetRoomsByIdsAsync(List<int> ids);
    }
}
