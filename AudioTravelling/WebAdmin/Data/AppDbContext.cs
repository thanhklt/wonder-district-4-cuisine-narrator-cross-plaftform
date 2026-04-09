using Microsoft.EntityFrameworkCore;
using WebAdmin.Models;

namespace WebAdmin.Data
{
    // Dùng để kết nối tới databases
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<StoreItem> StoreItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleID);

                entity.HasOne(u => u.Package)
                      .WithMany(p => p.Users)
                      .HasForeignKey(u => u.PackageID);

                entity.HasOne(u => u.BankAccount)
                      .WithMany(b => b.Users)
                      .HasForeignKey(u => u.BankAccountID);
            });

            // Store
            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(s => s.StoreID);

                entity.HasOne(s => s.Owner)
                      .WithMany(u => u.Stores)
                      .HasForeignKey(s => s.OwnerID);
            });

            // Gallery
            modelBuilder.Entity<Gallery>(entity =>
            {
                entity.HasKey(g => g.GalleryID);

                entity.HasOne(g => g.Store)
                      .WithMany(s => s.Galleries)
                      .HasForeignKey(g => g.StoreID);
            });

            // StoreItem
            modelBuilder.Entity<StoreItem>(entity =>
            {
                entity.HasKey(si => si.StoreItemID);

                entity.HasOne(si => si.Store)
                      .WithMany(s => s.StoreItems)
                      .HasForeignKey(si => si.StoreID);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.CartItemID);

                entity.HasOne(ci => ci.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(ci => ci.UserID);

                entity.HasOne(ci => ci.StoreItem)
                      .WithMany(si => si.CartItems)
                      .HasForeignKey(ci => ci.StoreItemID);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderID);

                entity.HasOne(o => o.Customer)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.OrderCustomerID);
            });

            // OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => od.OrderDetailID);

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderID);

                entity.HasOne(od => od.StoreItem)
                      .WithMany(si => si.OrderDetails)
                      .HasForeignKey(od => od.OrderDetailStoreItemID);
            });
        }
    }
}