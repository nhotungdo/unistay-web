using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Unistay_Web.Models.Room;
using Unistay_Web.Models.User;
using Unistay_Web.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Unistay_Web.Controllers
{
    public class RoomsController : Controller
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
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(string? address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.UserFullName = user.FullName;
                ViewBag.UserBirthYear = user.DateOfBirth?.Year;
                ViewBag.UserOccupation = user.Occupation;
                ViewBag.UserBio = user.Bio;
                ViewBag.UserAvatar = user.AvatarUrl;
                ViewBag.UserGender = user.Gender;
            }

            var room = new Room();
            if (!string.IsNullOrEmpty(address))
            {
                room.Address = address;
            }
            return View(room);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, List<IFormFile> imageFiles, List<string> selectedAmenities)
        {
            // Set OwnerId from current user
            var userId = _userManager.GetUserId(User);
            // ModelState may be invalid due to OwnerId being required but missing in form
            ModelState.Remove("OwnerId");
            room.OwnerId = userId ?? string.Empty;
            
            // Handle Amenities
            if (selectedAmenities != null && selectedAmenities.Any())
            {
                room.Amenities = string.Join(",", selectedAmenities);
            }

            // Set defaults
            room.CreatedAt = DateTime.Now;
            room.UpdatedAt = DateTime.Now;
            room.Status = "Available";
            room.ViewCount = 0;
            room.ContactCount = 0;

            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                
                // Handle Image Uploads
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    int order = 0;
                    foreach (var file in imageFiles)
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
                                CreatedAt = DateTime.Now
                            };
                            
                            _context.RoomImages.Add(roomImage);
                            order++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, Room room)
        {
            if (ModelState.IsValid)
            {
                // TODO: Update database
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(room);
        }

        public IActionResult Search(string? location, string? type, string? price)
        {
            // Redirect to Index page with search parameters
            // The new Index page has integrated search functionality
            return RedirectToAction(nameof(Index), new { location, type, price });
        }
    }
}
