using System;

namespace Api.Persistence.Entities
{
    public class PoiLocalization
    {
        public int LocalizationId { get; set; }

        public int PoiId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string AudioUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Poi Poi { get; set; } = null!;
    }
}