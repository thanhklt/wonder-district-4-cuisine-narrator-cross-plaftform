using System.Security.Claims;
using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Enums;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/pois")]
public class PoisController(IAppDbContext db, ILocalizationService localization) : ControllerBase
{
    // ── Tourist ─────────────────────────────────────────────
    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromHeader(Name = "X-Session-Token")] string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();
        var valid = await db.AccessSessions.AnyAsync(s => s.SessionToken == token && s.ExpiresAt > DateTime.UtcNow);
        if (!valid) return Unauthorized();

        var pois = await db.Pois
            .Where(p => p.Status == PoiStatus.Approved)
            .Include(p => p.Images)
            .Include(p => p.Localizations)
            .ToListAsync();

        return Ok(pois.Select(p => new
        {
            p.Id, p.Name, p.Lat, p.Lng, p.RadiusMeters, p.Priority,
            Images = p.Images.OrderBy(i => i.Order).Select(i => i.ImageUrl),
            Localizations = p.Localizations.Select(l => new { l.Language, l.TextContent, l.AudioUrl }),
        }));
    }

    // ── Owner ────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetOwnerPois()
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pois = await db.Pois
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Package)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(pois);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromBody] CreatePoiRequest req)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var package = await db.Packages.FindAsync(req.PackageId);
        if (package is null) return BadRequest(new { message = "Package not found" });

        var poi = new Poi
        {
            OwnerId = ownerId,
            Name = req.Name,
            Lat = req.Lat,
            Lng = req.Lng,
            RadiusMeters = package.RadiusMeters,
            Priority = package.Priority,
            PackageId = req.PackageId,
            Status = PoiStatus.Draft,
        };
        db.Pois.Add(poi);

        // Add Vietnamese localization
        db.PoiLocalizations.Add(new PoiLocalization
        {
            PoiId = poi.Id,
            Language = "vi",
            TextContent = req.Description,
        });

        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOwnerPois), new { id = poi.Id }, new { poi.Id, poi.Status });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePoiRequest req)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.Include(p => p.Localizations)
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId);
        if (poi is null) return NotFound();
        if (poi.Status == PoiStatus.Approved)
            return BadRequest(new { message = "Cannot edit an approved POI. Submit a new one." });

        poi.Name = req.Name;
        poi.Lat = req.Lat;
        poi.Lng = req.Lng;
        poi.UpdatedAt = DateTime.UtcNow;

        var viLoc = poi.Localizations.FirstOrDefault(l => l.Language == "vi");
        if (viLoc is not null) viLoc.TextContent = req.Description;

        await db.SaveChangesAsync();
        return Ok(new { poi.Id, poi.Status });
    }

    [HttpPatch("{id:guid}/submit")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId);
        if (poi is null) return NotFound();
        if (poi.Status != PoiStatus.Draft && poi.Status != PoiStatus.Rejected)
            return BadRequest(new { message = "Only Draft or Rejected POIs can be submitted" });

        poi.Status = PoiStatus.Pending;
        poi.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { poi.Id, poi.Status });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId);
        if (poi is null) return NotFound();
        db.Pois.Remove(poi);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Admin ────────────────────────────────────────────────
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var pois = await db.Pois
            .Where(p => p.Status == PoiStatus.Pending)
            .Include(p => p.Owner)
            .Include(p => p.Images)
            .Include(p => p.Localizations)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
        return Ok(pois);
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.FindAsync(id);
        if (poi is null) return NotFound();
        if (poi.Status != PoiStatus.Pending) return BadRequest(new { message = "POI is not pending" });

        poi.Status = PoiStatus.Approved;
        poi.UpdatedAt = DateTime.UtcNow;

        db.PoiApprovalLogs.Add(new PoiApprovalLog
        {
            PoiId = poi.Id,
            AdminId = adminId,
            Action = "Approved",
        });
        await db.SaveChangesAsync();

        // Trigger localization pipeline in background
        _ = Task.Run(() => localization.LocalizePoiAsync(poi.Id));

        return Ok(new { poi.Id, poi.Status });
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.FindAsync(id);
        if (poi is null) return NotFound();

        poi.Status = PoiStatus.Rejected;
        poi.UpdatedAt = DateTime.UtcNow;

        db.PoiApprovalLogs.Add(new PoiApprovalLog
        {
            PoiId = poi.Id,
            AdminId = adminId,
            Action = "Rejected",
            Note = req.Note,
        });
        await db.SaveChangesAsync();
        return Ok(new { poi.Id, poi.Status });
    }

    public record CreatePoiRequest(string Name, double Lat, double Lng, int PackageId, string Description);
    public record UpdatePoiRequest(string Name, double Lat, double Lng, string Description);
    public record RejectRequest(string? Note);
}
