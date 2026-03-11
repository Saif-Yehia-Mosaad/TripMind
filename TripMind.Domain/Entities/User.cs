using System;
using System.Collections.Generic;

namespace TripMind.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string? HomeGovernorate { get; set; }
        public string LanguagePreference { get; set; } = "AR";
        public bool RememberMe { get; set; }
        public bool IsActive { get; set; } = true;
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<SavedItinerary> SavedItineraries { get; set; } = new List<SavedItinerary>();
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        public ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
        public ICollection<ReviewVote> ReviewVotes { get; set; } = new List<ReviewVote>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}