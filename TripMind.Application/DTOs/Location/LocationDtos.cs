using System;
using TripMind.Domain.Enums;

namespace TripMind.Application.DTOs.Location
{
    public sealed class LocationSearchRequest
    {
        public string? Governorate { get; set; }
        public LocationCategory? Category { get; set; }
        public bool? HiddenGemsOnly { get; set; }
        public decimal? MaxPriceEgp { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public sealed class LocationResponse
    {
        public Guid LocationId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Governorate { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }
        public decimal? EntryFeeEgp { get; set; }
        public decimal? AvgPricePerNightEgp { get; set; }
        public decimal? AvgMealPriceEgp { get; set; }
        public string? OpeningHours { get; set; }
        public bool IsHiddenGem { get; set; }
        public float PopularityScore { get; set; }
        public float AvgRating { get; set; }
        public string? HiddenGemStory { get; set; }
    }
}
