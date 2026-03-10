using System;

namespace TripMind.Domain.Entities
{
    public class LocationFeature
    {
        public Guid LocationFeatureId { get; set; }
        public Guid LocationId { get; set; }
        public string FeatureKey { get; set; } = null!;
        public float FeatureValue { get; set; }

        public Location Location { get; set; } = null!;
    }
}
