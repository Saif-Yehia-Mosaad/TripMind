using System;

namespace TripMind.Domain.Entities
{
    public class FavoriteTrip
    {
        public Guid FavoriteTripId { get; set; }
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Trip Trip { get; set; } = null!;
    }
}