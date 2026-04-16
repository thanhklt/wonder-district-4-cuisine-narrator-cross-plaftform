using Api.Modules.Poi.DTOs;
using Api.Modules.Poi.Interfaces;
using Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Poi.Services;

public class PoiService : IPoiService
{
    private readonly AppDbContext _context;

    public PoiService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PoiListItemResponse>> GetActivePoisAsync()
    {
        return await _context.Pois
            .AsNoTracking()
            .Where(p => p.IsActive && p.ApprovalStatus.ToLower() == "approved")
            .Select(p => new PoiListItemResponse
            {
                PoiId = p.PoiId,
                Name = p.NameVi,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Radius = p.Radius,
                Priority = p.Priority,
                CoverImageUrl = p.Images
                    .Where(i => i.IsCover)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
                    ?? p.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<PoiDetailResponse?> GetPoiByIdAsync(int poiId)
    {
        return await _context.Pois
            .AsNoTracking()
            .Where(p => p.PoiId == poiId &&
                        p.IsActive &&
                        p.ApprovalStatus.ToLower() == "approved")
            .Select(p => new PoiDetailResponse
            {
                PoiId = p.PoiId,
                Name = p.NameVi,
                DescriptionVi = p.DescriptionVi,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Radius = p.Radius,
                Priority = p.Priority,
                Images = p.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}