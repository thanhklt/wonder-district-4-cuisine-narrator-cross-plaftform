using Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Poi> Pois { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PoiImage> PoiImages { get; set; }
        public DbSet<PoiLocalization> PoiLocalizations { get; set; }
        public DbSet<PoiApprovalLog> PoiApprovalLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Primary Keys không theo convention ---
            modelBuilder.Entity<PoiImage>()
                .HasKey(pi => pi.ImageId);

            modelBuilder.Entity<PoiLocalization>()
                .HasKey(pl => pl.LocalizationId);

            modelBuilder.Entity<PoiApprovalLog>()
                .HasKey(x => x.LogId);

                // --- Relationships ---

                // User - Role
                modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Poi - Owner
            modelBuilder.Entity<Poi>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Poi - Package
            modelBuilder.Entity<Poi>()
                .HasOne(p => p.Package)
                .WithMany(pk => pk.Pois)
                .HasForeignKey(p => p.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Poi - PoiImages
            modelBuilder.Entity<PoiImage>()
                .HasOne(pi => pi.Poi)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PoiId)
                .OnDelete(DeleteBehavior.Cascade);

            // Poi - PoiLocalizations
            modelBuilder.Entity<PoiLocalization>()
                .HasOne(pl => pl.Poi)
                .WithMany(p => p.Localizations)
                .HasForeignKey(pl => pl.PoiId)
                .OnDelete(DeleteBehavior.Cascade);

            // PoiApprovalLog
            modelBuilder.Entity<PoiApprovalLog>(entity =>
            {
                entity.HasOne(log => log.Poi)
                    .WithMany()
                    .HasForeignKey(log => log.PoiId);

                entity.HasOne(log => log.PerformedByAdmin)
                    .WithMany()
                    .HasForeignKey(log => log.PerformedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}