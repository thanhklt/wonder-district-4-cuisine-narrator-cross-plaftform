using Api.Repositories.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Repositories
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            context.Database.EnsureCreated();

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Owner" }
                );
                context.SaveChanges();
            }

            if (!context.Packages.Any())
            {
                context.Packages.AddRange(
                    new Package { Name = "Basic", Radius = 15, Priority = 0, Price = 0 },
                    new Package { Name = "Advanced", Radius = 30, Priority = 1, Price = 100000 },
                    new Package { Name = "Professional", Radius = 50, Priority = 2, Price = 250000 }
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Email = "admin@audiotravelling.com",
                    FullName = "System Admin",
                    PhoneNumber = "0901234567",
                    RoleID = 1,
                    UserStatus = 1,
                    CreatedDate = DateTime.UtcNow
                };
                admin.PasswordHash = passwordHasher.HashPassword(admin, "123456");

                var owner = new User
                {
                    Email = "owner@audiotravelling.com",
                    FullName = "Test Owner",
                    PhoneNumber = "0912345678",
                    RoleID = 2,
                    UserStatus = 1,
                    CreatedDate = DateTime.UtcNow
                };
                owner.PasswordHash = passwordHasher.HashPassword(owner, "123456");

                context.Users.AddRange(admin, owner);
                context.SaveChanges();

                // Seed some POIs
                if (!context.Pois.Any())
                {
                    var poi1 = new Poi
                    {
                        PoiName = "Quán Phở Hòa",
                        DescriptionVi = "Phở nổi tiếng",
                        Latitude = 10.7845,
                        Longitude = 106.6912,
                        Status = "Approved",
                        IsActive = true,
                        PackageId = 1,
                        OwnerID = owner.UserID,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    var poi2 = new Poi
                    {
                        PoiName = "Bánh Mì 362",
                        DescriptionVi = "Bánh mì Việt Nam",
                        Latitude = 10.7555,
                        Longitude = 106.6673,
                        Status = "Pending",
                        IsActive = false,
                        PackageId = 2,
                        OwnerID = owner.UserID,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    context.Pois.AddRange(poi1, poi2);
                    context.SaveChanges();
                }

                // Seed QrCodes
                if (!context.QrCodes.Any())
                {
                    var code1 = new QrCode
                    {
                        QrCodeValue = "QR-DEMO-001",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    context.QrCodes.Add(code1);
                    context.SaveChanges();

                    // Seed sessions
                    var session1 = new AccessSession
                    {
                        QrCodeID = code1.QrCodeID,
                        DeviceID = "device-1",
                        IssuedAt = DateTime.UtcNow.AddMinutes(-30),
                        ExpiredAt = DateTime.UtcNow.AddMinutes(30),
                        IsRevoked = false
                    };
                    context.AccessSessions.Add(session1);
                    context.SaveChanges();
                }
            }
        }
    }
}
