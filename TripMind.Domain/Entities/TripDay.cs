using System;
using System.Collections.Generic;

namespace TripMind.Domain.Entities
{
    public class TripDay
    {
        public Guid TripDayId { get; set; }
        public Guid TripId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }

        public Trip Trip { get; set; } = null!;
        public ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
    }
}
