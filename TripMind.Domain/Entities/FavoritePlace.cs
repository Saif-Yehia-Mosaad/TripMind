using System;

namespace TripMind.Domain.Entities
{
    public class FavoritePlace
    {
        public Guid FavoritePlaceId { get; set; }
        public Guid UserId { get; set; }
        public string PlaceId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
    }
}
