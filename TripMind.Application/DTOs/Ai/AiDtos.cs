using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripMind.Application.DTOs.Ai
{
    // ── Generate Plan ─────────────────────────────────────────────────────────
    public sealed class GeneratePlanRequest
    {
        private static readonly HashSet<string> AllowedCities = new(StringComparer.OrdinalIgnoreCase)
        {
            "Cairo","Giza","Alexandria","Luxor","Aswan",
            "Sharm El Sheikh","Hurghada","Port Said",
            "Ismailia","Marsa Matrouh","Fayoum"
        };

        private static readonly HashSet<string> AllowedInterests = new(StringComparer.Ordinal)
        {
            "Arts & Crafts","Bakery","Beaches & Water","Cafe","Entertainment",
            "History & Antiquities","Mosques & Churches","Music","Nature",
            "Nightlife","Outdoor","Park","Restaurants","Seafood",
            "Shopping","Street Food","Tourism","Waterfront"
        };

        [Required]
        [CustomValidation(typeof(GeneratePlanRequest), nameof(ValidateCity))]
        public string City { get; set; } = null!;

        [Required][Range(1, 7)] public int Days { get; set; }
        [Required][Range(1, 10000000)] public int Budget { get; set; }
        [Required][Range(1, 50)] public int People { get; set; }

        [Required]
        [CustomValidation(typeof(GeneratePlanRequest), nameof(ValidateInterests))]
        public List<string> Interests { get; set; } = new();

        public string? MustInclude { get; set; }

        public static ValidationResult? ValidateCity(string city, ValidationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(city) || !AllowedCities.Contains(city))
                return new ValidationResult(
                    $"City '{city}' is not supported. Allowed: " +
                    string.Join(", ", AllowedCities));
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateInterests(List<string> interests, ValidationContext ctx)
        {
            if (interests == null || interests.Count == 0)
                return new ValidationResult("Interests list cannot be empty.");
            var invalid = interests.Where(i => !AllowedInterests.Contains(i)).ToList();
            if (invalid.Any())
                return new ValidationResult(
                    $"Invalid interests: {string.Join(", ", invalid)}. " +
                    $"Allowed: {string.Join(", ", AllowedInterests)}");
            return ValidationResult.Success;
        }
    }

    // ── ChatBot ───────────────────────────────────────────────────────────────
    public sealed class ChatRequest
    {
        [Required] public string SessionId { get; set; } = null!;
        [Required] public string Message { get; set; } = null!;
        public ChatCollected Collected { get; set; } = new();
        public ChatCardAnswersRequest? CardAnswers { get; set; }
    }

    public sealed class ChatCollected
    {
        public string? Destination { get; set; }
        public int? Days { get; set; }
        public int? Budget { get; set; }
        public List<string> Interests { get; set; } = new();
        public int? People { get; set; }
        public List<string> MustInclude { get; set; } = new();
    }

    // ── EditBot ───────────────────────────────────────────────────────────────
    public sealed class EditRequest
    {
        [Required] public string TargetChange { get; set; } = null!;
        [Required] public string Destination { get; set; } = null!;
        [Required][Range(1, 7)] public int Days { get; set; }
        [Required][Range(1, 10000000)] public int Budget { get; set; }
        [Required][Range(1, 50)] public int People { get; set; }
        [Required] public List<string> Interests { get; set; } = new();
        public List<PlanItemRequest> ExistingPlan { get; set; } = new();
        public List<PlanItemRequest> Places { get; set; } = new();
        public List<ConversationTurn> Conversation { get; set; } = new();
    }

    public sealed class PlanItemRequest
    {
        [JsonPropertyName("place_id")]
        public string? PlaceId { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonConverter(typeof(DayFieldConverter))]
        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("city_en")]
        public string? CityEn { get; set; }

        [JsonPropertyName("lat")]
        public float Lat { get; set; }

        [JsonPropertyName("lng")]
        public float Lng { get; set; }

        [JsonPropertyName("rating")]
        public float Rating { get; set; }

        [JsonPropertyName("reviews_count")]
        public int ReviewsCount { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("photo_url")]
        public string? PhotoUrl { get; set; }

        [JsonPropertyName("image_urls")]
        public List<string> ImageUrls { get; set; } = new();

        [JsonPropertyName("maps_url")]
        public string? MapsUrl { get; set; }

        [JsonPropertyName("interests")]
        public List<string> Interests { get; set; } = new();
    }

    public sealed class ConversationTurn
    {
        [Required]
        [RegularExpression("^(user|assistant)$", ErrorMessage = "Role must be 'user' or 'assistant'.")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [Required]
        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;
    }

    // ── Recommendations ───────────────────────────────────────────────────────
    public sealed class HomeRequest
    {
        public string? City { get; set; }
        public int? Seed { get; set; }
    }

    public sealed class RecommendRequest
    {
        [Required] public List<string> SelectedCategories { get; set; } = new();
        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
        public int? Seed { get; set; }
        [Range(10, 500)] public int PoolSize { get; set; } = 150;
    }

    public sealed class SearchPlacesRequest
    {
        public string? Query { get; set; }
        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
    }

    public sealed class TopRatedRequest
    {
        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
    }

    public sealed class GetPlacesRequest
    {
        private static readonly HashSet<string> AllowedInterests = new(StringComparer.Ordinal)
        {
            "Arts & Crafts","Bakery","Beaches & Water","Cafe","Entertainment",
            "History & Antiquities","Mosques & Churches","Music","Nature",
            "Nightlife","Outdoor","Park","Restaurants","Seafood",
            "Shopping","Street Food","Tourism","Waterfront"
        };

        public List<string>? City { get; set; }
        public List<string>? Category { get; set; }

        [CustomValidation(typeof(GetPlacesRequest), nameof(ValidateInterests))]
        public List<string>? Interests { get; set; }

        [Range(0, 5)] public float? MinRating { get; set; }
        [Range(0, 5)] public float? MaxRating { get; set; }
        [Range(0, 100000)] public float? MinPrice { get; set; }
        [Range(0, 100000)] public float? MaxPrice { get; set; }
        public bool? HiddenGem { get; set; }

        [RegularExpression("^(rating|reviews|price|name)$",
            ErrorMessage = "SortBy must be: rating, reviews, price, or name.")]
        public string SortBy { get; set; } = "rating";

        [RegularExpression("^(asc|desc)$",
            ErrorMessage = "Order must be 'asc' or 'desc'.")]
        public string Order { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;

        public static ValidationResult? ValidateInterests(List<string>? interests, ValidationContext ctx)
        {
            if (interests == null || interests.Count == 0) return ValidationResult.Success;
            var invalid = interests.Where(i => !AllowedInterests.Contains(i)).ToList();
            if (invalid.Any())
                return new ValidationResult(
                    $"Invalid interests: {string.Join(", ", invalid)}. " +
                    $"Allowed: {string.Join(", ", AllowedInterests)}");
            return ValidationResult.Success;
        }
    }
}