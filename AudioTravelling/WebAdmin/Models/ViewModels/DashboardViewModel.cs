namespace WebAdmin.Models.ViewModels
{
    /// <summary>
    /// View model for Admin Dashboard page
    /// </summary>
    public class AdminDashboardViewModel
    {
        // ── Stat Cards ──
        public int TotalPoi { get; set; }
        public int PendingPoi { get; set; }
        public int ApprovedPoi { get; set; }
        public int RejectedPoi { get; set; }
        public int ActivePoi { get; set; }
        public int TotalQr { get; set; }
        public int TotalPackages { get; set; }
        public int TotalOwners { get; set; }
        public int TotalUsers { get; set; }

        // ── Recent POI pending approval ──
        public List<RecentPoiItem> RecentPendingPois { get; set; } = new();

        // ── Recent users ──
        public List<RecentUserItem> RecentUsers { get; set; } = new();
    }

    /// <summary>
    /// View model for Owner Dashboard page
    /// </summary>
    public class OwnerDashboardViewModel
    {
        public int TotalPoi { get; set; }
        public int PendingPoi { get; set; }
        public int ApprovedPoi { get; set; }
        public int RejectedPoi { get; set; }
        public int ActivePoi { get; set; }

        public List<RecentPoiItem> RecentPois { get; set; } = new();
    }

    // ── Shared sub-models ──
    public class RecentPoiItem
    {
        public int PoiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? ImageUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class RecentUserItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
