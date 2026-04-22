using AudioTravelling.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.Core.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Package> Packages { get; }
    DbSet<Poi> Pois { get; }
    DbSet<PoiImage> PoiImages { get; }
    DbSet<PoiLocalization> PoiLocalizations { get; }
    DbSet<PoiApprovalLog> PoiApprovalLogs { get; }
    DbSet<AccessCode> AccessCodes { get; }
    DbSet<AccessSession> AccessSessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
