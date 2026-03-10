using System;

namespace TripMind.Domain.Entities
{
    public class UserPreference
    {
        public Guid UserPreferenceId { get; set; }
        public Guid UserId { get; set; }
        public string PreferenceKey { get; set; } = null!;
        public string PreferenceValue { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
