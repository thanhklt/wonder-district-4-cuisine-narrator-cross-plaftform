using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.Models.ViewModels;

namespace WebAdmin.Controllers
{
    [Authorize(Policy = "OwnerOnly")]
    public class OwnerController : Controller
    {
        // ── GET: /Owner/Dashboard ──
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Tổng quan";

            // Get current owner's ID from claims
            // var ownerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var model = new OwnerDashboardViewModel
            {
                TotalPoi = 5,
                PendingPoi = 2,
                ApprovedPoi = 2,
                RejectedPoi = 1,
                ActivePoi = 2,

                RecentPois = new List<RecentPoiItem>
                {
                    new() {
                        PoiId = 1,
                        Name = "Ốc Đào Vĩnh Khánh",
                        PackageName = "Chuyên nghiệp",
                        Status = "Approved",
                        ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-3)
                    },
                    new() {
                        PoiId = 5,
                        Name = "Bún Mắm Cô Sáu",
                        PackageName = "Nâng cao",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddHours(-6)
                    },
                    new() {
                        PoiId = 3,
                        Name = "Lẩu Dê Hùng Vương",
                        PackageName = "Cơ bản",
                        Status = "Pending",
                        ImageUrl = "https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-1)
                    },
                    new() {
                        PoiId = 7,
                        Name = "Cơm Tấm Thiên Hương",
                        PackageName = "Cơ bản",
                        Status = "Approved",
                        ImageUrl = "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-5)
                    },
                    new() {
                        PoiId = 10,
                        Name = "Chè Bưởi Cô Tư",
                        PackageName = "Cơ bản",
                        Status = "Rejected",
                        ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=100&h=100&fit=crop",
                        SubmittedAt = DateTime.Now.AddDays(-7)
                    }
                }
            };

            return View(model);
        }

        // ── GET: /Owner/PoiManagement ──
        public IActionResult PoiManagement()
        {
            ViewData["Title"] = "Quản lý POI";
            return View();
        }
    }
}
