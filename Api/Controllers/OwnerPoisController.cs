using Api.Models;
using Api.Models.Entities;
using Api.Repositories;

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
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Invalid user id in token.");
            return userId;
        }

        private string? BuildImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ownerId = GetUserId();
            var pois = await _context.Pois.Include(p => p.Package).Include(p => p.Images).Where(p => p.OwnerID == ownerId).ToListAsync();

            var result = pois.Select(p => new PoiDto
            {
                PoiId = p.PoiID, PoiName = p.PoiName, DescriptionVi = p.DescriptionVi,
                Latitude = p.Latitude, Longitude = p.Longitude, Radius = p.Package.Radius,
                Priority = p.Package.Priority, Status = p.Status.ToLower(), StatusText = p.Status,
                IsActive = p.IsActive, PackageId = p.PackageId, PackageName = p.Package.Name,
                OwnerId = p.OwnerID, CreatedDate = p.CreatedDate, UpdatedDate = p.UpdatedDate,
                Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new PoiImageDto
                {
                    ImageID = i.ImageID, IsCover = i.IsCover, DisplayOrder = i.DisplayOrder,
                    ImageUrl = BuildImageUrl(i.ImageUrl) ?? string.Empty
                }).ToList(),
                ImageUrl = BuildImageUrl(p.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl)
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.Include(p => p.Package).Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            return Ok(new PoiDto
            {
                PoiId = poi.PoiID, PoiName = poi.PoiName, DescriptionVi = poi.DescriptionVi,
                Latitude = poi.Latitude, Longitude = poi.Longitude, Radius = poi.Package.Radius,
                Priority = poi.Package.Priority, Status = poi.Status.ToLower(), StatusText = poi.Status,
                IsActive = poi.IsActive, PackageId = poi.PackageId, PackageName = poi.Package.Name,
                OwnerId = poi.OwnerID, CreatedDate = poi.CreatedDate, UpdatedDate = poi.UpdatedDate,
                Images = poi.Images.OrderBy(i => i.DisplayOrder).Select(i => new PoiImageDto
                {
                    ImageID = i.ImageID, IsCover = i.IsCover, DisplayOrder = i.DisplayOrder,
                    ImageUrl = BuildImageUrl(i.ImageUrl) ?? string.Empty
                }).ToList(),
                ImageUrl = BuildImageUrl(poi.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl ?? poi.Images.FirstOrDefault()?.ImageUrl)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePoiRequest request)
        {
            var ownerId = GetUserId();
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
                PackageId = request.PackageId, OwnerID = ownerId,
                Status = "Pending", IsActive = false,
                CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow
            };

            _context.Pois.Add(poi);
            await _context.SaveChangesAsync();

            if (request.ImageFile != null && request.ImageFile.Length > 0)
                _context.PoiImages.Add(new PoiImage { PoiID = poi.PoiID, ImageUrl = SaveImageFile(request.ImageFile), IsCover = true, DisplayOrder = 1 });
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                _context.PoiImages.Add(new PoiImage { PoiID = poi.PoiID, ImageUrl = request.ImageUrl, IsCover = true, DisplayOrder = 1 });

            var addOrder = 2;
            foreach (var extra in request.AdditionalImages ?? [])
            {
                if (extra.Length > 0 && addOrder <= 4)
                {
                    _context.PoiImages.Add(new PoiImage { PoiID = poi.PoiID, ImageUrl = SaveImageFile(extra), IsCover = false, DisplayOrder = addOrder++ });
                }
            }

            await _context.SaveChangesAsync();
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

            poi.PoiName = request.PoiName ?? string.Empty; 
            poi.DescriptionVi = request.DescriptionVi ?? string.Empty;
            poi.Latitude = request.Latitude; poi.Longitude = request.Longitude;
            poi.PackageId = request.PackageId; poi.Status = "Pending";
            poi.IsActive = false; poi.UpdatedDate = DateTime.UtcNow;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return StatusCode(403, new { message = "Owner không có quyền xóa POI. Chỉ Admin được phép xóa POI." });
        }

        [HttpPatch("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            poi.Status = "Pending"; poi.IsActive = false; poi.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Submitted successfully" });
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromForm] AddPoiImageRequest request)
        {
            var ownerId = GetUserId();
            var poi = await _context.Pois.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
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
            var ownerId = GetUserId();
            var poi = await _context.Pois.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
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
            var ownerId = GetUserId();
            var poi = await _context.Pois.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PoiID == id && p.OwnerID == ownerId);
            if (poi == null) return NotFound();

            var target = poi.Images.FirstOrDefault(i => i.ImageID == imageId);
            if (target == null) return NotFound();

            foreach (var img in poi.Images) img.IsCover = false;
            target.IsCover = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã đặt ảnh bìa." });
        }
    }
}
