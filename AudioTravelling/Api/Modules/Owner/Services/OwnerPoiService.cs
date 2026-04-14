namespace Api.Modules.Owner.Services;

using Api.Modules.Owner.DTOs;
using Api.Modules.Owner.Interfaces;
using Api.Persistence.Entities; // nếu entity nằm ở module Poi
using Api.Persistence;
using Microsoft.EntityFrameworkCore;

public class OwnerPoiService : IOwnerPoiService
{
    private readonly AppDbContext _context;

    public OwnerPoiService(AppDbContext context)
    {
        _context = context;
    }

    // logic tao 1 owner
    public async Task<PoiResponse> CreatePoiAsync(CreatePoiRequest request, int ownerId)
    {
        var package = await _context.Packages
            .FirstOrDefaultAsync(p => p.PackageId == request.PackageId);

        if (package == null)
            throw new Exception("Package not found");

        var poi = new Poi
        {
            OwnerId = ownerId,
            NameVi = request.NameVi,
            DescriptionVi = request.DescriptionVi,
            Latitude = request.Latitude,
            Longitude = request.Longitude,

            PackageId = package.PackageId,
            Radius = package.Radius,
            Priority = package.Priority,

            ApprovalStatus = "pending",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Pois.Add(poi);
        await _context.SaveChangesAsync();

        return new PoiResponse
        {
            PoiId = poi.PoiId,
            NameVi = poi.NameVi,
            DescriptionVi = poi.DescriptionVi,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            Radius = poi.Radius,
            Priority = poi.Priority,
            ApprovalStatus = poi.ApprovalStatus
        };
    }

    // Logic get poi tu jwt
    public async Task<List<OwnerPoiItemResponse>> GetMyPoisAsync(int ownerId)
    {
        return await _context.Pois
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new OwnerPoiItemResponse
            {
                PoiId = p.PoiId,
                NameVi = p.NameVi,
                DescriptionVi = p.DescriptionVi,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Radius = p.Radius,
                Priority = p.Priority,
                ApprovalStatus = p.ApprovalStatus,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }
}