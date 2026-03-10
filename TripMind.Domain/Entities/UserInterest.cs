using System;

namespace TripMind.Domain.Entities
{
    public class UserInterest
    {
        public Guid UserInterestId { get; set; }
        public Guid UserId { get; set; }
        public string InterestTag { get; set; } = null!;
        public int Weight { get; set; } = 1;

        public User User { get; set; } = null!;
    }
}
