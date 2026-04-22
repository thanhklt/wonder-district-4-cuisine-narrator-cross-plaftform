using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Enums;
using AudioTravelling.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<PoiImage> PoiImages => Set<PoiImage>();
    public DbSet<PoiLocalization> PoiLocalizations => Set<PoiLocalization>();
    public DbSet<PoiApprovalLog> PoiApprovalLogs => Set<PoiApprovalLog>();
    public DbSet<AccessCode> AccessCodes => Set<AccessCode>();
    public DbSet<AccessSession> AccessSessions => Set<AccessSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AccessSession>()
            .HasIndex(s => s.SessionToken)
            .IsUnique();

        b.Entity<AccessSession>()
            .HasIndex(s => s.ExpiresAt);

        b.Entity<Poi>()
            .Property(p => p.Status)
            .HasConversion<string>();

        b.Entity<PoiApprovalLog>()
            .HasOne(l => l.Admin)
            .WithMany()
            .HasForeignKey(l => l.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Poi>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Pois)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Roles
        b.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Owner" }
        );

        // Seed Packages
        b.Entity<Package>().HasData(
            new Package { Id = 1, Name = "Cơ bản", RadiusMeters = 15, Priority = 0, Price = 0, Description = "Bán kính 15m, ưu tiên thấp nhất" },
            new Package { Id = 2, Name = "Nâng cao", RadiusMeters = 30, Priority = 1, Price = 200000, Description = "Bán kính 30m, ưu tiên trung bình" },
            new Package { Id = 3, Name = "Chuyên nghiệp", RadiusMeters = 50, Priority = 2, Price = 500000, Description = "Bán kính 50m, ưu tiên cao nhất" }
        );

        // Seed default Admin user — password: Admin@123
        b.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Email = "admin@audiotravelling.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = 1,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Email = "owner@audiotravelling.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
                RoleId = 2,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            }
        );
    }
}
