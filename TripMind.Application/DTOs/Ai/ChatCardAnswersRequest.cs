using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TripMind.Application.DTOs.Ai
{
    public sealed class ChatCardAnswersRequest
    {
        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("days")]
        public int? Days { get; set; }

        [JsonPropertyName("budget")]
        public int? Budget { get; set; }

        [JsonPropertyName("interests")]
        public List<string> Interests { get; set; } = new();

        [JsonPropertyName("people")]
        public int? People { get; set; }

        [JsonPropertyName("must_include")]
        public List<string> MustInclude { get; set; } = new();
    }
}