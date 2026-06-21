using System;

namespace TripMind.Domain.Entities
{
    public class FavoritePlace
    {
        public Guid FavoritePlaceId { get; set; }
        public Guid UserId { get; set; }

        public string PlaceId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string? CityEn { get; set; }
        public string? Category { get; set; }
        public float Rating { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}