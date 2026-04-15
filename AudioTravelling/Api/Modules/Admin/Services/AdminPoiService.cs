using Api.Modules.Admin.DTOs;
using Api.Modules.Admin.Interfaces;
using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Admin.Services;

public class AdminPoiService : IAdminPoiService
{
    private readonly AppDbContext _dbContext;

    public AdminPoiService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PendingPoiResponse>> GetPendingPoisAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pois
            .AsNoTracking()
            .Where(x => x.ApprovalStatus.ToLower() == "pending")
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PendingPoiResponse
            {
                PoiId = x.PoiId,
                OwnerId = x.OwnerId,
                NameVi = x.NameVi,
                DescriptionVi = x.DescriptionVi,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Radius = x.Radius,
                Priority = x.Priority,
                PackageId = x.PackageId,
                ApprovalStatus = x.ApprovalStatus,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task ApprovePoiAsync(int poiId, int adminUserId, CancellationToken cancellationToken = default)
    {
        var poi = await _dbContext.Pois
            .FirstOrDefaultAsync(x => x.PoiId == poiId, cancellationToken);

        if (poi is null)
            throw new Exception("POI not found.");

        if (!string.Equals(poi.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Only pending POI can be approved.");

        poi.ApprovalStatus = "approved";
        poi.IsActive = true;
        poi.UpdatedAt = DateTime.UtcNow;

        _dbContext.PoiApprovalLogs.Add(new PoiApprovalLog
        {
            PoiId = poi.PoiId,
            PerformedBy = adminUserId,
            Action = "approved",
            Note = "Approved by admin.",
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectPoiAsync(int poiId, int adminUserId, string note, CancellationToken cancellationToken = default)
    {
        var poi = await _dbContext.Pois
            .FirstOrDefaultAsync(x => x.PoiId == poiId, cancellationToken);

        if (poi is null)
            throw new Exception("POI not found.");

        if (!string.Equals(poi.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Only pending POI can be rejected.");

        if (string.IsNullOrWhiteSpace(note))
            throw new Exception("Reject note is required.");

        poi.ApprovalStatus = "rejected";
        poi.IsActive = false;
        poi.UpdatedAt = DateTime.UtcNow;

        _dbContext.PoiApprovalLogs.Add(new PoiApprovalLog
        {
            PoiId = poi.PoiId,
            PerformedBy = adminUserId,
            Action = "rejected",
            Note = note.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}