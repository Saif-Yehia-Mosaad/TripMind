using System;
using System.Collections.Generic;

namespace TripMind.Domain.Entities
{
    public class TourPackage
    {
        public Guid TourPackageId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public string Governorate { get; set; } = null!;
        public int DurationDays { get; set; }
        public decimal PricePerPersonEgp { get; set; }
        public string? PhotoUrl { get; set; }
        public float AvgRating { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public ICollection<TourPackageLocation> Locations { get; set; } = new List<TourPackageLocation>();
    }

    public class TourPackageLocation
    {
        public Guid TourPackageLocationId { get; set; }
        public Guid TourPackageId { get; set; }
        public Guid LocationId { get; set; }
        public int DayNumber { get; set; }
        public int SequenceOrder { get; set; }

        public TourPackage TourPackage { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}