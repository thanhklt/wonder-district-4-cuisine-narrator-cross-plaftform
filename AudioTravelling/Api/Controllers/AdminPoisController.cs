using Api.Persistence;
using Api.Persistence.Entities;
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

        public AdminPoisController(AppDbContext context)
        {
            _context = context;
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
                .Where(p => p.Status == "Pending")
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
                    OwnerName = p.Owner.FullName,
                    OwnerEmail = p.Owner.Email,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Images = new List<string>()
                }).ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.Pois
                .Include(p => p.Package)
                .Include(p => p.Owner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var poisList = await query.ToListAsync();

            var pois = poisList.Select(p => new PoiDto
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
                    OwnerName = p.Owner.FullName,
                    OwnerEmail = p.Owner.Email,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    Images = new List<string>()
                }).ToList();

            return Ok(pois);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Pois
                .Include(poi => poi.Package)
                .Include(poi => poi.Owner)
                .FirstOrDefaultAsync(poi => poi.PoiID == id);

            if (p == null) return NotFound();

            return Ok(new PoiDto
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
                OwnerName = p.Owner.FullName,
                OwnerEmail = p.Owner.Email,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                Images = new List<string>()
            });
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
    }

    public class RejectPoiRequest
    {
        public string Note { get; set; } = string.Empty;
    }
}
