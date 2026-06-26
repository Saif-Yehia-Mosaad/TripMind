using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TripMind.Application.DTOs.Ai
{
    public sealed class PlaceFiltersRequest
    {
        [JsonPropertyName("city_en")]
        public string? CityEn { get; set; }

        [JsonPropertyName("category")]
        public List<string>? Category { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }

        [JsonPropertyName("min_rating")]
        public float? MinRating { get; set; }

        [JsonPropertyName("max_rating")]
        public float? MaxRating { get; set; }

        [JsonPropertyName("min_price")]
        public float? MinPrice { get; set; }

        [JsonPropertyName("max_price")]
        public float? MaxPrice { get; set; }

        [JsonPropertyName("hidden_gem")]
        public bool? HiddenGem { get; set; }

        [JsonPropertyName("sort_by")]
        public string? SortBy { get; set; }

        [JsonPropertyName("order")]
        public string? Order { get; set; }
    }
}
