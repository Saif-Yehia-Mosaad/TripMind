using System;
using System.Collections.Generic;

namespace TripMind.Application.DTOs.TourPackage
{
    public sealed class TourPackageResponse
    {
        public Guid TourPackageId { get; init; }
        public string NameEn { get; init; } = null!;
        public string NameAr { get; init; } = null!;
        public string? DescriptionEn { get; init; }
        public string? DescriptionAr { get; init; }
        public string Governorate { get; init; } = null!;
        public int DurationDays { get; init; }
        public decimal PricePerPersonEgp { get; init; }
        public string? PhotoUrl { get; init; }
        public float AvgRating { get; init; }
        public List<TourPackageLocationResponse> Locations { get; init; } = new();
    }

    public sealed class TourPackageLocationResponse
    {
        public Guid LocationId { get; init; }
        public string NameEn { get; init; } = null!;
        public string NameAr { get; init; } = null!;
        public string Category { get; init; } = null!;
        public int DayNumber { get; init; }
        public int SequenceOrder { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    public sealed class TourPackageSearchRequest
    {
        public string? Governorate { get; set; }
        public int? MinDays { get; set; }
        public int? MaxDays { get; set; }
        public decimal? MaxPricePerPerson { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}