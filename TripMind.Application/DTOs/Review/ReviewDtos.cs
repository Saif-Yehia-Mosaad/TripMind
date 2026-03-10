using System;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Review
{
    public sealed class AddReviewRequest
    {
        [Required] public Guid LocationId { get; set; }
        [Required][Range(1, 5)] public int Rating { get; set; }
        [MaxLength(2000)] public string? ReviewText { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime? VisitedAt { get; set; }
    }

    public sealed class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public string? PhotoUrl { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime? VisitedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
