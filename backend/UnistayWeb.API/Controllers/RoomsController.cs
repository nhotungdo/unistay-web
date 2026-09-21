using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UnistayWeb.DAL.Models.Room;
using UnistayWeb.DAL.Models.User;
using UnistayWeb.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UnistayWeb.API.Controllers
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

    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomsController(UserManager<UserProfile> userManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] string? location, [FromQuery] string? price)
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
            
            // Manually fetch images and owner
            var roomIds = rooms.Select(r => r.Id).ToList();
            var images = await _context.RoomImages.Where(i => roomIds.Contains(i.RoomId)).ToListAsync();
            var ownerIds = rooms.Select(r => r.OwnerId).Distinct().ToList();
            var owners = await _context.Users.Where(u => ownerIds.Contains(u.Id)).ToListAsync();

            var result = rooms.Select(r => new {
                Room = r,
                Images = images.Where(i => i.RoomId == r.Id).ToList(),
                Owner = owners.FirstOrDefault(u => u.Id == r.OwnerId)
            });

            return Ok(result);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound(new { message = "Không tìm thấy phòng." });
            }

            var images = await _context.RoomImages.Where(i => i.RoomId == id).ToListAsync();
            var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == room.OwnerId);

            return Ok(new {
                Room = room,
                Images = images,
                Owner = owner
            });
        }

        // POST: api/Rooms
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> CreateRoom([FromForm] CreateRoomDto dto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "rooms");
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

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditRoom(int id, [FromBody] Room room)
        {
            if (id != room.Id)
            {
                return BadRequest(new { message = "ID không khớp." });
            }

            var userId = _userManager.GetUserId(User);
            var existingRoom = await _context.Rooms.FindAsync(id);

            if (existingRoom == null) return NotFound();
            if (existingRoom.OwnerId != userId) return Forbid();

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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id)) return NotFound();
                else throw;
            }

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var userId = _userManager.GetUserId(User);
            var room = await _context.Rooms.FindAsync(id);

            if (room == null) return NotFound();
            if (room.OwnerId != userId) return Forbid();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa phòng thành công." });
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}
