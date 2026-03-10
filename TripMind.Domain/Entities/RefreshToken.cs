using System;

namespace TripMind.Domain.Entities
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

        public User User { get; set; } = null!;
    }
}
