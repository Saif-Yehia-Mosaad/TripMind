using System;

namespace TripMind.Domain.Entities
{
    public class ReviewVote
    {
        public Guid ReviewVoteId { get; set; }
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public bool IsHelpful { get; set; }
        public DateTime VotedAt { get; set; }

        public Review Review { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
