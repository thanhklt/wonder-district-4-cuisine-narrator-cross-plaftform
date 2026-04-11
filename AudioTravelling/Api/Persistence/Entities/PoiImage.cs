using System;

namespace Api.Persistence.Entities
{
    public class PoiImage
    {
        public int ImageId { get; set; }

        public int PoiId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsCover { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Poi Poi { get; set; } = null!;
    }
}