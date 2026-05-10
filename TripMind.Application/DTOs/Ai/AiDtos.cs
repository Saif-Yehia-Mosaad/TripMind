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
}