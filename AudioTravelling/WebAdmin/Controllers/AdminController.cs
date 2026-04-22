using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Models.ViewModels;

namespace WebAdmin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        // ── GET: /Admin/Dashboard ──
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["PendingCount"] = 4; // Mock pending count for sidebar badge

            var model = new AdminDashboardViewModel
            {
                TotalPoi = 12,
                PendingPoi = 4,
                ApprovedPoi = 6,
                RejectedPoi = 2,
                ActivePoi = 5,
                TotalQr = 10,
                TotalPackages = 3,
                TotalOwners = 3,
                TotalUsers = 8,

                RecentPendingPois = new List<RecentPoiItem>
                {
                    new() {
                        PoiId = 1,
                        Name = "Ốc Đào Vĩnh Khánh",
                        OwnerName = "Trần Thị Owner",
                        PackageName = "Chuyên nghiệp",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddHours(-2)
                    },
                    new() {
                        PoiId = 2,
                        Name = "Bánh Tráng Trộn Cô Ba",
                        OwnerName = "Lê Minh Quán",
                        PackageName = "Nâng cao",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddHours(-5)
                    },
                    new() {
                        PoiId = 3,
                        Name = "Lẩu Dê Hùng Vương",
                        OwnerName = "Trần Thị Owner",
                        PackageName = "Cơ bản",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-1)
                    },
                    new() {
                        PoiId = 4,
                        Name = "Hải Sản Năm Đảnh",
                        OwnerName = "Lê Minh Quán",
                        PackageName = "Nâng cao",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1559339352-11d035aa65de?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-1).AddHours(-3)
                    }
                },

                RecentUsers = new List<RecentUserItem>
                {
                    new() {
                        UserId = 5,
                        FullName = "Phạm Văn Hoàng",
                        Email = "hoang@gmail.com",
                        Role = "User",
                        AvatarUrl = "https://i.pravatar.cc/150?img=12",
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    new() {
                        UserId = 6,
                        FullName = "Nguyễn Thị Lan",
                        Email = "lan@gmail.com",
                        Role = "User",
                        AvatarUrl = "https://i.pravatar.cc/150?img=9",
                        CreatedAt = DateTime.Now.AddDays(-2)
                    },
                    new() {
                        UserId = 3,
                        FullName = "Lê Minh Quán",
                        Email = "owner2@audiotravelling.vn",
                        Role = "Owner",
                        AvatarUrl = "https://i.pravatar.cc/150?img=8",
                        CreatedAt = DateTime.Now.AddDays(-3)
                    }
                }
            };

            return View(model);
        }

        // ── GET: /Admin/QrManagement ──
        public IActionResult QrManagement()
        {
            ViewData["Title"] = "Quản lý QR";
            ViewData["PendingCount"] = 4;
            return View();
        }

        // ── GET: /Admin/PackageManagement ──
        public IActionResult PackageManagement()
        {
            ViewData["Title"] = "Quản lý Package";
            ViewData["PendingCount"] = 4;
            return View();
        }

        // ── GET: /Admin/PoiManagement ──
        public IActionResult PoiManagement()
        {
            ViewData["Title"] = "Quản lý POI";
            ViewData["PendingCount"] = 4;
            return View();
        }

        // ── GET: /Admin/PoiApproval ──
        public IActionResult PoiApproval()
        {
            ViewData["Title"] = "Duyệt POI";
            ViewData["PendingCount"] = 4;
            return View();
        }

        // ── GET: /Admin/UserManagement ──
        public IActionResult UserManagement()
        {
            ViewData["Title"] = "Quản lý User";
            ViewData["PendingCount"] = 4;
            return View();
        }
    }
}
