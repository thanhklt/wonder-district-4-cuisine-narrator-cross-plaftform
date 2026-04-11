using System;

namespace Api.Persistence.Entities
{
    public class PoiApprovalLog
    {
        public int LogId { get; set; }

        public int PoiId { get; set; }

        public int PerformedBy { get; set; }

        public string Action { get; set; } = string.Empty;
        // approve / reject / update

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        // 🔥 Navigation properties

        public Poi Poi { get; set; } = null!;

        public User PerformedByAdmin { get; set; } = null!;
    }
}