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

        [Required]
        [MaxLength(100)]
        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateCity))]
        public string City { get; set; } = null!;

        [Required][Range(1, 7)] public int Days { get; set; }
        [Required][Range(1, 10000000)] public int Budget { get; set; }
        [Required][Range(1, 50)] public int People { get; set; }

        [Required]
        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateDisplayInterests))]
        public List<string> Interests { get; set; } = new();

        [MaxLength(500)]
        public string? MustInclude { get; set; }
    }

    // ── ChatBot ───────────────────────────────────────────────────────────────
    public sealed class ChatRequest
    {
        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = null!;
        public ChatCollected Collected { get; set; } = new();
        public ChatCardAnswersRequest? CardAnswers { get; set; }
    }

    public sealed class ChatCollected
    {
        [MaxLength(100)]
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
        [Required]
        [MaxLength(500)]
        public string TargetChange { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = null!;
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
        [MaxLength(4000)]
        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;
    }

    // ── Recommendations ───────────────────────────────────────────────────────
    public sealed class HomeRequest
    {

        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateCity))]
        public string? City { get; set; }

        public int? Seed { get; set; }
    }

    /// <summary>
    /// Request for POST /places/recommend.
    ///
    /// IMPORTANT — Recommendation engine behavior (per AI team):
    /// 1. All places are ranked by cosine similarity between
    ///    `selected_categories`/interests and each place's data — highest
    ///    similarity first.
    /// 2. The top `pool_size` results (default 50) are taken from that
    ///    ranked list.
    /// 3. The pool is then shuffled using `seed`. Same seed → same shuffle
    ///    order every time (stable pagination). Different/omitted seed →
    ///    different shuffle.
    /// 4. If `pool_size` is large (e.g. 500), the pool can include places
    ///    with a similarity score of 0 (i.e. places that don't actually
    ///    match `selected_categories` at all) — and after shuffling, one of
    ///    those irrelevant places can end up first. This is expected
    ///    behavior of the AI ranking engine, NOT a backend bug — the
    ///    backend only forwards `pool_size`/`seed` and returns the response
    ///    as-is (JsonElement passthrough, no re-sorting on our side).
    ///
    /// Backend-side guarantee added here: `pool_size` must be >= `limit`,
    /// otherwise there aren't enough items in the pool to even fill one
    /// page of results after shuffling.
    /// </summary>
    public sealed class RecommendRequest : IValidatableObject
    {
        [Required]
        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateDisplayInterests))]
        public List<string> SelectedCategories { get; set; } = new();

        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)]
        public int Limit { get; set; } = 10;

        public int? Seed { get; set; }

        [Range(10, 500)]
        public int PoolSize { get; set; } = 150;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PoolSize < Limit)
                yield return new ValidationResult(
                    $"PoolSize ({PoolSize}) must be greater than or equal to Limit ({Limit}). " +
                    "Otherwise there aren't enough ranked places to shuffle and fill the requested page.",
                    new[] { nameof(PoolSize) });
        }
    }

    public sealed class SearchPlacesRequest
    {
        [MaxLength(500)]
        public string? Query { get; set; }
        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
    }
    public sealed class NearbyRequest
    {
        public float UserLat { get; set; }
        public float UserLng { get; set; }
        public float? RadiusKm { get; set; }

        public PlaceFiltersRequest? Filters { get; set; }
        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]


        public int Page { get; set; } = 1;
        [Range(1, 50)]
        public int Limit { get; set; } = 10;
    }
    public sealed class TopRatedRequest
    {
        public PlaceFiltersRequest? Filters { get; set; }

        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
    }

    public sealed class GetPlacesRequest
    {
        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateCities))]
        public List<string>? City { get; set; }

        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidatePlaceCategories))]
        public List<string>? Category { get; set; }

        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateDisplayInterests))]
        public List<string>? Interests { get; set; }

        [Range(0, 5)] public float? MinRating { get; set; }
        [Range(0, 5)] public float? MaxRating { get; set; }
        [Range(0, 100000)] public float? MinPrice { get; set; }
        [Range(0, 100000)] public float? MaxPrice { get; set; }
        public bool? HiddenGem { get; set; }

        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateSortBy))]
        public string SortBy { get; set; } = "rating";

        [CustomValidation(typeof(AiValidation), nameof(AiValidation.ValidateOrder))]
        public string Order { get; set; } = "desc";

        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]
        public int Page { get; set; } = 1;

        [Range(1, 50)] public int Limit { get; set; } = 10;
    }
}