using System;

namespace TripMind.Domain.Entities
{
    public class SavedItinerary
    {
        public Guid SavedItineraryId { get; set; }
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public string? CustomName { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime SavedAt { get; set; }

        public User User { get; set; } = null!;
        public Trip Trip { get; set; } = null!;
    }
}
