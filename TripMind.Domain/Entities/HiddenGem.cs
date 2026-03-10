using System;

namespace TripMind.Domain.Entities
{
    public class HiddenGem
    {
        public Guid HiddenGemId { get; set; }
        public Guid LocationId { get; set; }
        public string? Story { get; set; }
        public int AnnualVisitors { get; set; }
        public float QualityScore { get; set; }
        public DateTime TaggedAt { get; set; }

        public Location Location { get; set; } = null!;
    }
}
