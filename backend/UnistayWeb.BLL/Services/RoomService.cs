using Microsoft.EntityFrameworkCore;
using UnistayWeb.BLL.ViewModels;
using UnistayWeb.DAL.Data;
using UnistayWeb.DAL.Models.Marketplace;
using UnistayWeb.DAL.Models.Room;

namespace UnistayWeb.BLL.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<RoomService> _logger;

        public RoomService(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<RoomService> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<RoomListItemDto>> GetRoomsAsync(string? location, string? price)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(r => r.Address.Contains(location));
            }

            if (!string.IsNullOrEmpty(price))
            {
                if (price == "under2m") query = query.Where(r => r.Price < 2000000);
                else if (price == "2m-4m") query = query.Where(r => r.Price >= 2000000 && r.Price <= 4000000);
                else if (price == "above4m") query = query.Where(r => r.Price > 4000000);
            }

            var rooms = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            var roomIds = rooms.Select(r => r.Id).ToList();
            var images = await _context.RoomImages.Where(i => roomIds.Contains(i.RoomId)).ToListAsync();
            var ownerIds = rooms.Select(r => r.OwnerId).Distinct().ToList();
            var owners = await _context.Users.Where(u => ownerIds.Contains(u.Id)).ToListAsync();

            return rooms.Select(r => new RoomListItemDto
            {
                Room = r,
                Images = images.Where(i => i.RoomId == r.Id).ToList(),
                Owner = owners.FirstOrDefault(u => u.Id == r.OwnerId)
            }).ToList();
        }

        public async Task<RoomDetailDto?> GetRoomAsync(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
                return null;

            var images = await _context.RoomImages.Where(i => i.RoomId == id).ToListAsync();
            var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == room.OwnerId);

            return new RoomDetailDto
            {
                Room = room,
                Images = images,
                Owner = owner
            };
        }

        public async Task<Room> CreateRoomAsync(CreateRoomDto dto, string userId, string webRootPath)
        {
            var room = new Room
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Deposit = dto.Deposit,
                Area = dto.Area,
                Address = dto.Address,
                MaxOccupants = dto.MaxOccupants,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Rules = dto.Rules,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Available",
                ViewCount = 0,
                ContactCount = 0
            };

            if (dto.SelectedAmenities != null && dto.SelectedAmenities.Any())
            {
                room.Amenities = string.Join(",", dto.SelectedAmenities);
            }

            _context.Add(room);
            await _context.SaveChangesAsync();

            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(webRootPath, "images", "rooms");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                int order = 0;
                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var roomImage = new RoomImage
                        {
                            RoomId = room.Id,
                            ImageUrl = "/images/rooms/" + uniqueFileName,
                            IsPrimary = (order == 0),
                            DisplayOrder = order,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.RoomImages.Add(roomImage);
                        order++;
                    }
                }
                await _context.SaveChangesAsync();
            }

            return room;
        }

        public async Task<bool> EditRoomAsync(int id, Room room, string userId)
        {
            if (id != room.Id)
                return false;

            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
                return false;
            if (existingRoom.OwnerId != userId)
                return false;

            existingRoom.Title = room.Title;
            existingRoom.Description = room.Description;
            existingRoom.Price = room.Price;
            existingRoom.Deposit = room.Deposit;
            existingRoom.Area = room.Area;
            existingRoom.Address = room.Address;
            existingRoom.MaxOccupants = room.MaxOccupants;
            existingRoom.Latitude = room.Latitude;
            existingRoom.Longitude = room.Longitude;
            existingRoom.Amenities = room.Amenities;
            existingRoom.Rules = room.Rules;
            existingRoom.Status = room.Status;
            existingRoom.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(int id, string userId)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return false;
            if (room.OwnerId != userId)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        public async Task<List<Room>> GetRoomsByIdsAsync(List<int> ids)
        {
            return await _context.Rooms.Where(r => ids.Contains(r.Id)).ToListAsync();
        }
    }
}
