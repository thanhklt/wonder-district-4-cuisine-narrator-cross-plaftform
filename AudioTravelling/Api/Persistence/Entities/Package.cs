using System.Collections.Generic;

namespace Api.Persistence.Entities
{
    public class Package
    {
        public int PackageId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Radius { get; set; }

        public int Priority { get; set; }

        public decimal Price { get; set; }

        // Navigation property
        public ICollection<Poi> Pois { get; set; } = new List<Poi>();
    }
}