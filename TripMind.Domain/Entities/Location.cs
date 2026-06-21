using System;
using System.Collections.Generic;
using TripMind.Domain.Enums;

namespace TripMind.Domain.Entities
{
    public class Location
    {
        public Guid LocationId { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public LocationCategory Category { get; set; }
        public string Governorate { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public decimal? EntryFeeEgp { get; set; }
        public decimal? AvgPricePerNightEgp { get; set; }
        public decimal? AvgMealPriceEgp { get; set; }
        public string? OpeningHours { get; set; }
        public bool IsHiddenGem { get; set; }
        public float PopularityScore { get; set; }
        public float AvgRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
        public ICollection<LocationFeature> LocationFeatures { get; set; } = new List<LocationFeature>();
        public HiddenGem? HiddenGem { get; set; }
    }
}
