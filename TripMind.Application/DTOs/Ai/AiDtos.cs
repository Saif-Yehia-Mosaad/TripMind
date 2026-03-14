using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Ai
{
    public sealed class AiSearchRequest
    {
        [Required] public decimal Budget { get; set; }
        [Required][Range(1, 50)] public int Members { get; set; }
        public string? Governorate { get; set; }
        public string? Preferences { get; set; }
    }

    public sealed class AiSearchResponse
    {
        public IEnumerable<AiPlaceResult> Places { get; init; } = new List<AiPlaceResult>();
        public IEnumerable<AiPlaceResult> HiddenGems { get; init; } = new List<AiPlaceResult>();
        public string? Summary { get; init; }
    }

    public sealed class AiPlaceResult
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public string? Governorate { get; init; }
        public string? Category { get; init; }
        public double? Rating { get; init; }
        public string? PhotoUrl { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public IEnumerable<string> Reviews { get; init; } = new List<string>();
    }
}