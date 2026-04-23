using System.Security.Claims;
using AudioTravelling.API.DTOs;
using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Enums;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/pois")]
public class PoisController(IAppDbContext db, IServiceScopeFactory scopeFactory, IConfiguration config) : ControllerBase
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

        return Ok(pois.Select(p => new PoiSummaryResponse(
            p.Id, p.Name, p.Lat, p.Lng, p.RadiusMeters, p.Priority,
            p.Images.OrderBy(i => i.Order).Select(i => i.ImageUrl),
            p.Localizations.Select(l => new LocalizationDto(l.Language, l.TextContent, l.AudioUrl))
        )));
    }

    // ── Owner / Admin ────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetOwnerPois()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        var pois = await db.Pois
            .Where(p => isAdmin || p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Lat = p.Lat,
                Lng = p.Lng,
                RadiusMeters = p.RadiusMeters,
                Priority = p.Priority,
                Status = p.Status.ToString(),
                PackageId = p.PackageId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Images = p.Images
                    .OrderBy(i => i.Order)
                    .Select(i => new { Id = i.Id, ImageUrl = i.ImageUrl, Order = i.Order })
                    .ToList(),
                Localizations = p.Localizations
                    .Select(l => new { l.Language, l.TextContent })
                    .ToList()
            })
            .ToListAsync();

        return Ok(pois);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePoiRequest req)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var package = await db.Packages.FindAsync(req.PackageId);
        if (package is null) return BadRequest(new { message = "Package not found" });

        var isAdmin = User.IsInRole("Admin");
        var poi = new Poi
        {
            OwnerId = ownerId,
            Name = req.Name,
            Lat = req.Lat,
            Lng = req.Lng,
            RadiusMeters = package.RadiusMeters,
            Priority = package.Priority,
            PackageId = req.PackageId,
            Status = isAdmin ? PoiStatus.Approved : PoiStatus.Draft,
        };
        db.Pois.Add(poi);

        db.PoiLocalizations.Add(new PoiLocalization
        {
            PoiId = poi.Id,
            Language = "vi",
            TextContent = req.Description,
        });

        await db.SaveChangesAsync();

        // Admin tạo POI → dịch + generate TTS ngay
        // Owner tạo POI → chỉ dịch text, TTS sẽ chạy sau khi được approve
        RunInBackground(svc => isAdmin
            ? svc.LocalizePoiAsync(poi.Id)
            : svc.TranslateOnlyAsync(poi.Id));

        return CreatedAtAction(nameof(GetOwnerPois), new { id = poi.Id }, new PoiStatusResponse(poi.Id, poi.Status));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePoiRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var poi = await db.Pois.Include(p => p.Localizations)
            .FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.OwnerId == userId));
        if (poi is null) return NotFound();

        poi.Name = req.Name;
        poi.Lat = req.Lat;
        poi.Lng = req.Lng;
        poi.UpdatedAt = DateTime.UtcNow;

        var viLoc = poi.Localizations.FirstOrDefault(l => l.Language == "vi");
        var descriptionChanged = viLoc is not null && viLoc.TextContent != req.Description;
        if (viLoc is not null) viLoc.TextContent = req.Description;

        if (descriptionChanged)
        {
            // Xóa bản dịch cũ để dịch lại với nội dung mới
            var oldTranslations = poi.Localizations.Where(l => l.Language != "vi").ToList();
            db.PoiLocalizations.RemoveRange(oldTranslations);
        }

        await db.SaveChangesAsync();

        if (descriptionChanged)
            RunInBackground(svc => svc.TranslateOnlyAsync(poi.Id));

        return Ok(new PoiStatusResponse(poi.Id, poi.Status));
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
        return Ok(new PoiStatusResponse(poi.Id, poi.Status));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var poi = await db.Pois.FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.OwnerId == userId));
        if (poi is null) return NotFound();
        db.Pois.Remove(poi);
        await db.SaveChangesAsync();

        var audioDir = Path.Combine(
            config["AUDIO_STORAGE_PATH"] ?? "/storage/audio",
            id.ToString());
        if (Directory.Exists(audioDir))
            Directory.Delete(audioDir, recursive: true);

        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────
    private void RunInBackground(Func<ILocalizationService, Task> work)
    {
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
            await work(svc);
        });
    }

    // ── Admin ────────────────────────────────────────────────
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var pois = await db.Pois
                .Where(p => p.Status == PoiStatus.Pending || p.Status == PoiStatus.Draft)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Lat = p.Lat,
                    Lng = p.Lng,
                    RadiusMeters = p.RadiusMeters,
                    Priority = p.Priority,
                    Status = p.Status,
                    PackageId = p.PackageId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,

                    Owner = p.Owner == null ? null : new
                    {
                        Id = p.Owner.Id,
                        Email = p.Owner.Email
                    },

                    Images = p.Images
                        .OrderBy(i => i.Order)
                        .Select(i => new
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            Order = i.Order
                        })
                        .ToList(),

                    Localizations = p.Localizations
                        .Select(l => new
                        {
                            Id = l.Id,
                            Language = l.Language,
                            TextContent = l.TextContent,
                            AudioUrl = l.AudioUrl,
                            CreatedAt = l.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(pois);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Failed to load pending POIs",
                detail = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var poi = await db.Pois.FindAsync(id);
        if (poi is null) return NotFound();
        if (poi.Status != PoiStatus.Pending && poi.Status != PoiStatus.Draft)
            return BadRequest(new { message = "POI is not waiting for approval" });

        poi.Status = PoiStatus.Approved;
        poi.UpdatedAt = DateTime.UtcNow;

        db.PoiApprovalLogs.Add(new PoiApprovalLog
        {
            PoiId = poi.Id,
            AdminId = adminId,
            Action = "Approved",
        });
        await db.SaveChangesAsync();

        RunInBackground(svc => svc.LocalizePoiAsync(poi.Id));

        return Ok(new PoiStatusResponse(poi.Id, poi.Status));
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
        return Ok(new PoiStatusResponse(poi.Id, poi.Status));
    }
}
