using System;

namespace TripMind.Application.DTOs.Itinerary
{
    public sealed class SavedItineraryResponse
    {
        public Guid SavedItineraryId { get; set; }
        public string? CustomName { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime SavedAt { get; set; }
        public Guid TripId { get; set; }
        public string Destination { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public string Status { get; set; } = null!;
    }

    public record SaveItineraryRequest(Guid TripId, string? CustomName, bool IsFavorite = false);
}
