using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripMind.Application.DTOs.Trip
{
    public sealed class TripReviewWithUserResponse
    {
        public Guid TripReviewId { get; init; }
        public Guid TripId { get; init; }
        public Guid UserId { get; init; }
        public string? DisplayName { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
