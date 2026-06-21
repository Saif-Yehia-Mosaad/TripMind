using System;

namespace TripMind.Domain.Entities
{
    public class TripReview
    {
        public Guid TripReviewId { get; set; }
        public Guid TripId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Trip Trip { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}