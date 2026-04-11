using Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        }
    }
}