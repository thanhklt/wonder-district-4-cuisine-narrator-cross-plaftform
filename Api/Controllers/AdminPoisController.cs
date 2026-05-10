using Api.Models;
using Api.Models.Entities;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/admin/pois")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPoisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWebHostEnvironment _env;

        public AdminPoisController(AppDbContext context, IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
        {
            _context = context;
            _scopeFactory = scopeFactory;
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var pois = await _context.Pois
                .Include(p => p.Package)
                .Include(p => p.Owner)
                .Include(p => p.Images)
                .Where(p => p.Status == "Pending")
                .ToListAsync();

            var result = pois.Select(p => MapToDto(p)).ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.Pois
                .Include(p => p.Package)
                .Include(p => p.Owner)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var poisList = await query.ToListAsync();

            var pois = poisList.Select(p => MapToDto(p)).ToList();

            return Ok(pois);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Pois
                .Include(poi => poi.Package)
                .Include(poi => poi.Owner)
                .Include(poi => poi.Images)
                .FirstOrDefaultAsync(poi => poi.PoiID == id);

            if (p == null) return NotFound();

            return Ok(MapToDto(p));
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();

            poi.Status = "Approved";
            poi.IsActive = true;
            poi.UpdatedDate = DateTime.UtcNow;

            var log = new PoiApprovalLog
            {
                PoiID = id,
                PerformedBy = GetUserId(),
                Action = "approve",
                Note = "Approved by Admin",
                CreatedDate = DateTime.UtcNow
            };
            _context.PoiApprovalLogs.Add(log);

            await _context.SaveChangesAsync();

            // Chay localization + TTS pipeline trong background
            var capturedId = id;
            var capturedFactory = _scopeFactory;
            _ = Task.Run(async () =>
            {
                await using var scope = capturedFactory.CreateAsyncScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<LocalizationPipeline>();
                await pipeline.LocalizePoiAsync(capturedId);
            });

            return Ok(new { message = "Approved successfully" });
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectPoiRequest request)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();

            poi.Status = "Rejected";
            poi.IsActive = false;
            poi.UpdatedDate = DateTime.UtcNow;

            var log = new PoiApprovalLog
            {
                PoiID = id,
                PerformedBy = GetUserId(),
                Action = "reject",
                Note = request.Note,
                CreatedDate = DateTime.UtcNow
            };
            _context.PoiApprovalLogs.Add(log);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Rejected successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AdminCreatePoiRequest request)
        {
            var adminId = GetUserId();
            var package = await _context.Packages.FindAsync(request.PackageId);
            if (package == null) return BadRequest("Invalid PackageId");

            var hasCoverImage = (request.ImageFile != null && request.ImageFile.Length > 0)
                                || !string.IsNullOrWhiteSpace(request.ImageUrl);
            if (!hasCoverImage)
                return BadRequest(new { message = "Ảnh bìa là bắt buộc khi tạo POI." });

            var poi = new Poi
            {
                PoiName = request.PoiName ?? string.Empty, 
                DescriptionVi = request.DescriptionVi ?? string.Empty,
                Latitude = request.Latitude, Longitude = request.Longitude,
                PackageId = request.PackageId, OwnerID = request.OwnerId ?? adminId,
                Status = "Approved", IsActive = true,
                CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow
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
                    await request.ImageFile.CopyToAsync(stream);

                _context.PoiImages.Add(new PoiImage { PoiID = poi.PoiID, ImageUrl = "/images/pois/" + fileName, IsCover = true, DisplayOrder = 1 });
                await _context.SaveChangesAsync();
            }
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                _context.PoiImages.Add(new PoiImage { PoiID = poi.PoiID, ImageUrl = request.ImageUrl, IsCover = true, DisplayOrder = 1 });
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Created successfully", poiId = poi.PoiID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] AdminUpdatePoiRequest request)
        {
            var poi = await _context.Pois.Include(p => p.Images).FirstOrDefaultAsync(p => p.PoiID == id);
            if (poi == null) return NotFound();

            var package = await _context.Packages.FindAsync(request.PackageId);
            if (package == null) return BadRequest("Invalid PackageId");

            poi.PoiName = request.PoiName ?? string.Empty; 
            poi.DescriptionVi = request.DescriptionVi ?? string.Empty;
            poi.Latitude = request.Latitude; poi.Longitude = request.Longitude;
            poi.PackageId = request.PackageId;
            if (request.OwnerId.HasValue) poi.OwnerID = request.OwnerId.Value;
            
            poi.UpdatedDate = DateTime.UtcNow;

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "pois");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.ImageFile.CopyToAsync(stream);

                var oldImage = poi.Images.FirstOrDefault();
                if (oldImage != null) oldImage.ImageUrl = "/images/pois/" + fileName;
                else poi.Images.Add(new PoiImage { ImageUrl = "/images/pois/" + fileName, IsCover = true, DisplayOrder = 1 });
            }
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                var oldImage = poi.Images.FirstOrDefault();
                if (oldImage != null) oldImage.ImageUrl = request.ImageUrl;
                else poi.Images.Add(new PoiImage { ImageUrl = request.ImageUrl, IsCover = true, DisplayOrder = 1 });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var poi = await _context.Pois.FindAsync(id);
            if (poi == null) return NotFound();

            poi.IsActive = !poi.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Toggled successfully", isActive = poi.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var poi = await _context.Pois
                .Include(p => p.Images)
                .Include(p => p.Localizations)
                .Include(p => p.ApprovalLogs)
                .FirstOrDefaultAsync(p => p.PoiID == id);

            if (poi == null) return NotFound("Không tìm thấy POI.");

            // Xóa các record con trước để tránh FK constraint
            _context.PoiImages.RemoveRange(poi.Images);
            _context.PoiLocalizations.RemoveRange(poi.Localizations);
            _context.PoiApprovalLogs.RemoveRange(poi.ApprovalLogs);
            _context.Pois.Remove(poi);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa POI.", poiId = id });
        }
        private string? BuildImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty + imageUrl; // keep original online URL
            }

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            if (!imageUrl.StartsWith("/"))
                imageUrl = "/" + imageUrl;

            return $"{baseUrl}{imageUrl}";
        }

        private string SaveImageFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "pois");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
            return "/images/pois/" + fileName;
        }

        private PoiDto MapToDto(Poi p)
        {
            var coverImageUrl = p.Images != null && p.Images.Any() ? (p.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl) : null;

            return new PoiDto
            {
                PoiId = p.PoiID,
                PoiName = p.PoiName,
                DescriptionVi = p.DescriptionVi,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Radius = p.Package?.Radius ?? 0,
                Priority = p.Package?.Priority ?? 0,
                Status = p.Status.ToLower(),
                StatusText = p.Status,
                IsActive = p.IsActive,
                PackageId = p.PackageId,
                PackageName = p.Package?.Name ?? "",
                OwnerId = p.OwnerID,
                OwnerName = p.Owner?.FullName ?? "",
                OwnerEmail = p.Owner?.Email ?? "",
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                ImageUrl = BuildImageUrl(coverImageUrl),
                Images = p.Images != null
                    ? p.Images.OrderBy(i => i.DisplayOrder).Select(i => new PoiImageDto
                    {
                        ImageID = i.ImageID, IsCover = i.IsCover, DisplayOrder = i.DisplayOrder,
                        ImageUrl = BuildImageUrl(i.ImageUrl) ?? string.Empty
                    }).ToList()
                    : new List<PoiImageDto>()
            };
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromForm] AddPoiImageRequest request)
        {
            var poi = await _context.Pois.Include(p => p.Images).FirstOrDefaultAsync(p => p.PoiID == id);
            if (poi == null) return NotFound();

            if (!request.IsCover && poi.Images.Count(i => !i.IsCover) >= 3)
                return BadRequest(new { message = "Tối đa 3 ảnh phụ cho mỗi POI." });

            string? imageUrl = null;
            if (request.ImageFile != null && request.ImageFile.Length > 0)
                imageUrl = SaveImageFile(request.ImageFile);
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                imageUrl = request.ImageUrl;

            if (imageUrl == null) return BadRequest(new { message = "Cần cung cấp file ảnh hoặc URL." });

            if (request.IsCover)
                foreach (var img in poi.Images) img.IsCover = false;

            var nextOrder = poi.Images.Any() ? poi.Images.Max(i => i.DisplayOrder) + 1 : 1;
            _context.PoiImages.Add(new PoiImage { PoiID = id, ImageUrl = imageUrl, IsCover = request.IsCover, DisplayOrder = nextOrder });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm ảnh thành công." });
        }

        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int id, int imageId)
        {
            var poi = await _context.Pois.Include(p => p.Images).FirstOrDefaultAsync(p => p.PoiID == id);
            if (poi == null) return NotFound();

            var image = poi.Images.FirstOrDefault(i => i.ImageID == imageId);
            if (image == null) return NotFound();

            var wasCover = image.IsCover;
            _context.PoiImages.Remove(image);
            await _context.SaveChangesAsync();

            if (wasCover)
            {
                var next = await _context.PoiImages.Where(i => i.PoiID == id).OrderBy(i => i.DisplayOrder).FirstOrDefaultAsync();
                if (next != null) { next.IsCover = true; await _context.SaveChangesAsync(); }
            }

            return Ok(new { message = "Đã xóa ảnh." });
        }

        [HttpPatch("{id}/images/{imageId}/cover")]
        public async Task<IActionResult> SetCover(int id, int imageId)
        {
            var poi = await _context.Pois.Include(p => p.Images).FirstOrDefaultAsync(p => p.PoiID == id);
            if (poi == null) return NotFound();

            var target = poi.Images.FirstOrDefault(i => i.ImageID == imageId);
            if (target == null) return NotFound();

            foreach (var img in poi.Images) img.IsCover = false;
            target.IsCover = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã đặt ảnh bìa." });
        }
    }

    public class AdminCreatePoiRequest
    {
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PackageId { get; set; }
        public int? OwnerId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class AdminUpdatePoiRequest
    {
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PackageId { get; set; }
        public int? OwnerId { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
    }
}
