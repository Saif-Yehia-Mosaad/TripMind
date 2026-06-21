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
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? HomeGovernorate { get; set; }
        public string LanguagePreference { get; set; } = "AR";
        public bool RememberMe { get; set; }
        public bool IsActive { get; set; } = true;
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationOtp { get; set; }
        public DateTime? EmailOtpExpiry { get; set; }
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorOtp { get; set; }
        public DateTime? TwoFactorOtpExpiry { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        public ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}