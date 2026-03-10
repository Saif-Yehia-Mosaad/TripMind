using System;

namespace TripMind.Domain.Entities
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; }
        public Guid? UserId { get; set; }
        public string EventType { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Details { get; set; }
        public bool Success { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}
