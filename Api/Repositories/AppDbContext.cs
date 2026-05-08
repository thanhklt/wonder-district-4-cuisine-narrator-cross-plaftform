using Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Poi> Pois { get; set; }
        public DbSet<PoiImage> PoiImages { get; set; }
        public DbSet<PoiLocalization> PoiLocalizations { get; set; }
        public DbSet<PoiApprovalLog> PoiApprovalLogs { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<QrCode> QrCodes { get; set; }
        public DbSet<AccessSession> AccessSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Poi>().ToTable("Pois");
            modelBuilder.Entity<PoiImage>().ToTable("PoiImages");
            modelBuilder.Entity<PoiLocalization>().ToTable("PoiLocalizations");
            modelBuilder.Entity<PoiApprovalLog>().ToTable("PoiApprovalLogs");
            modelBuilder.Entity<Package>().ToTable("Packages");
            modelBuilder.Entity<QrCode>().ToTable("QrCodes");
            modelBuilder.Entity<AccessSession>().ToTable("AccessSessions");

            // Cấu hình các mối quan hệ (cascade) nếu cần
            modelBuilder.Entity<Poi>(entity =>
            {
                entity.Property(e => e.Status)
                    .HasColumnName("Status")
                    .HasMaxLength(20)
                    .IsRequired();
            });

            modelBuilder.Entity<Poi>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Pois)
                .HasForeignKey(p => p.OwnerID)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa user nếu còn POI

            modelBuilder.Entity<Poi>()
                .HasOne(p => p.Package)
                .WithMany()
                .HasForeignKey(p => p.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PoiImage>()
                .HasOne(pi => pi.Poi)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PoiID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoiLocalization>()
                .HasOne(pl => pl.Poi)
                .WithMany(p => p.Localizations)
                .HasForeignKey(pl => pl.PoiID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoiApprovalLog>()
                .HasOne(pal => pal.Poi)
                .WithMany(p => p.ApprovalLogs)
                .HasForeignKey(pal => pal.PoiID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AccessSession>()
                .HasOne(s => s.QrCode)
                .WithMany(a => a.Sessions)
                .HasForeignKey(s => s.QrCodeID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
