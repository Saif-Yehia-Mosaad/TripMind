using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Ai
{
    public sealed class AiSearchRequest
    {
        [Required] public string City { get; set; } = null!;
        [Required][Range(1, 30)] public int Days { get; set; }
        [Required][Range(1, 10000000)] public decimal Budget { get; set; }
        public List<string> Interests { get; set; } = new();
    }
    public sealed class RecommendRequest
    {
        public List<string> UserInterests { get; set; } = new();
        public int TopN { get; set; } = 10;
        public int PoolSize { get; set; } = 30;
        public string? CityFilter { get; set; }
    }
}