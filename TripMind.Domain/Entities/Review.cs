using System;
using System.Collections.Generic;
using TripMind.Domain.Enums;

namespace TripMind.Domain.Entities
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid LocationId { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public string? PhotoUrl { get; set; }
        public int HelpfulCount { get; set; }
        public bool Reported { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;
        public DateTime? VisitedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Location Location { get; set; } = null!;
        public ICollection<ReviewVote> ReviewVotes { get; set; } = new List<ReviewVote>();
    }
}
