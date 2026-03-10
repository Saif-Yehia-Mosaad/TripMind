using System;

namespace TripMind.Domain.Entities
{
    public class TripLocation
    {
        public Guid TripLocationId { get; set; }
        public Guid TripId { get; set; }
        public Guid TripDayId { get; set; }
        public Guid LocationId { get; set; }
        public int DayNumber { get; set; }
        public string TimeSlot { get; set; } = null!;
        public int VisitDurationMinutes { get; set; }
        public int SequenceOrder { get; set; }

        public Trip Trip { get; set; } = null!;
        public TripDay TripDay { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}
