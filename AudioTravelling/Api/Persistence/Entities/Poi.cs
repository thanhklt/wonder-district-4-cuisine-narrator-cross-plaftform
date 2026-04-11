using System;
using System.Collections.Generic;

namespace Api.Persistence.Entities
{
    public class Poi
    {
        public int PoiId { get; set; }

        public int OwnerId { get; set; }

        public string NameVi { get; set; } = string.Empty;

        public string DescriptionVi { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int Radius { get; set; }

        public int Priority { get; set; }

        public int PackageId { get; set; }

        public string ApprovalStatus { get; set; } = "pending";

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // 🔥 Navigation properties

        public User Owner { get; set; } = null!;

        public Package Package { get; set; } = null!;

        public ICollection<PoiImage> Images { get; set; } = new List<PoiImage>();

        public ICollection<PoiLocalization> Localizations { get; set; } = new List<PoiLocalization>();
    }
}