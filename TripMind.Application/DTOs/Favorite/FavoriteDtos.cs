using System;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Favorite
{
    public sealed class FavoritePlaceRequest
    {
        [Required] public string PlaceId { get; set; } = null!;
        [Required] public string Name { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? CityEn { get; set; }
        public string? Category { get; set; }
        public float Rating { get; set; }
    }

    public sealed class FavoritePlaceResponse
    {
        public Guid FavoritePlaceId { get; init; }
        public string PlaceId { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? PhotoUrl { get; init; }
        public string? CityEn { get; init; }
        public string? Category { get; init; }
        public float Rating { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed class FavoriteTripResponse
    {
        public Guid FavoriteTripId { get; init; }
        public Guid TripId { get; init; }
        public string Destination { get; init; } = null!;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int DurationDays { get; init; }
        public string Status { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
    }
}