using System;
using System.Collections.Generic;
using TripMind.Domain.Enums;

namespace TripMind.Domain.Entities
{
    public class Trip
    {
        public Guid TripId { get; set; }
        public Guid UserId { get; set; }

        public string? Title { get; set; }

        public string DestinationGovernorate { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DurationDays { get; set; }

        public int People { get; set; }
        public int TotalBudgetEgp { get; set; }
        public int TotalCost { get; set; }

        public string? SessionId { get; set; }
        public string? CollectedJson { get; set; }
        public string? PlanJson { get; set; }

        public TripStatus Status { get; set; } = TripStatus.Draft;
        public bool IsPublic { get; set; }
        public string? ShareToken { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<TripReview> TripReviews { get; set; } = new List<TripReview>();
    }
}
