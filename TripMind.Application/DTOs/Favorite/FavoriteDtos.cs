using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TripMind.Application.DTOs.Favorite
{
    public sealed class FavoritePlaceRequest
    {
        [Required]
        [MaxLength(200)]
        public string PlaceId { get; set; } = null!;
    }

    public sealed class FavoritePlaceResponse
    {
        public Guid FavoritePlaceId { get; init; }
        public string PlaceId { get; init; } = null!;
        public JsonElement Place { get; init; }
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
