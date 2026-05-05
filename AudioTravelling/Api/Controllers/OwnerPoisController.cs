using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/owner/pois")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class OwnerPoisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public OwnerPoisController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value;

            if (!int.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user id in token.");
            }

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ownerId = GetUserId();
            var pois = await _context.Pois
                .Include(p => p.Package)
                .Where(p => p.OwnerID == ownerId)
                .ToListAsync();

            var result = pois.Select(p => new PoiDto
                {
                    PoiId = p.PoiID,
                    PoiName = p.PoiName,
                    DescriptionVi = p.DescriptionVi,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Radius = p.Package.Radius,
                    Priority = p.Package.Priority,
                    Status = p.Status.ToLower(),
                    StatusText = p.Status,
                    IsActive = p.IsActive,
                    PackageId = p.PackageId,
                    PackageName = p.Package.Name,
                    OwnerId = p.OwnerID,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Images = p.Images.Select(i => i.ImageUrl).ToList(),
                    ImageUrl = p.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl
                }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois
                .Include(p => p.Package)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);

            if (poi == null) return NotFound();

            return Ok(new PoiDto
            {
                PoiId = poi.PoiID,
                PoiName = poi.PoiName,
                DescriptionVi = poi.DescriptionVi,
                Latitude = poi.Latitude,
                Longitude = poi.Longitude,
                Radius = poi.Package.Radius,
                Priority = poi.Package.Priority,
                Status = poi.Status.ToLower(),
                StatusText = poi.Status,
                IsActive = poi.IsActive,
                PackageId = poi.PackageId,
                PackageName = poi.Package.Name,
                OwnerId = poi.OwnerID,
                CreatedDate = poi.CreatedDate,
                UpdatedDate = poi.UpdatedDate,
                Images = poi.Images.Select(i => i.ImageUrl).ToList(),
                ImageUrl = poi.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? poi.Images.FirstOrDefault()?.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePoiRequest request)
        {
            var ownerId = GetUserId();
            var package = await _context.Packages.FindAsync(request.PackageId);
            if (package == null) return BadRequest("Invalid PackageId");

            var poi = new Poi
            {
                PoiName = request.PoiName,
                DescriptionVi = request.DescriptionVi,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PackageId = request.PackageId,
                OwnerID = ownerId,
                Status = "Pending",
                IsActive = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Pois.Add(poi);
            await _context.SaveChangesAsync();

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "pois");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }
                var poiImage = new PoiImage
                {
                    PoiID = poi.PoiID,
                    ImageUrl = "/images/pois/" + fileName,
                    IsCover = true,
                    DisplayOrder = 1
                };
                _context.PoiImages.Add(poiImage);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Created successfully", poiId = poi.PoiID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePoiRequest request)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.Include(p => p.Images).FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            var package = await _context.Packages.FindAsync(request.PackageId);
            if (package == null) return BadRequest("Invalid PackageId");

            poi.PoiName = request.PoiName;
            poi.DescriptionVi = request.DescriptionVi;
            poi.Latitude = request.Latitude;
            poi.Longitude = request.Longitude;
            poi.PackageId = request.PackageId;
            poi.Status = "Pending";
            poi.IsActive = false;
            poi.UpdatedDate = DateTime.UtcNow;

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "pois");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }
                
                var oldImage = poi.Images.FirstOrDefault();
                if (oldImage != null)
                {
                    oldImage.ImageUrl = "/images/pois/" + fileName;
                }
                else
                {
                    poi.Images.Add(new PoiImage { ImageUrl = "/images/pois/" + fileName, IsCover = true, DisplayOrder = 1 });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            // Soft delete
            poi.IsActive = false;
            poi.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        [HttpPatch("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            poi.Status = "Pending";
            poi.IsActive = false;
            poi.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Submitted successfully" });
        }
    }

    public class PoiDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }

    public class CreatePoiRequest
    {
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PackageId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class UpdatePoiRequest : CreatePoiRequest
    {
    }
}
