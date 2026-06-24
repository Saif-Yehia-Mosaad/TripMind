using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using TripMind.Domain.Enums;

namespace TripMind.Application.DTOs.Trip
{
    public sealed class CreateTripRequest
    {
        public string? Title { get; set; }
        public string? DestinationGovernorate { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("people")]
        public int? People { get; set; }

        [JsonPropertyName("totalBudgetEgp")]
        [Required]
        public int TotalBudgetEgp { get; set; }

        public int? TotalCost { get; set; }
        public JsonElement? Plan { get; set; }
        public JsonElement? Collected { get; set; }
        public string? SessionId { get; set; }
        public bool? IsPublic { get; set; }
    }

    public sealed class UpdateTripRequest
    {
        public string? Title { get; set; }

        public string? DestinationGovernorate { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? People { get; set; }

        [JsonPropertyName("totalBudgetEgp")]
        public int? TotalBudgetEgp { get; set; }   // هنا فاضل int? لأنه partial update، بس مفيش Budget تاني

        public int? TotalCost { get; set; }

        public JsonElement? Plan { get; set; }
        public JsonElement? Collected { get; set; }
        public string? SessionId { get; set; }
        public bool? IsPublic { get; set; }
    }

    public sealed class RenameTripRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
    }


    public sealed class TripSearchRequest
    {
        public TripStatus? Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 20;
    }

    public sealed class TripResponse
    {
        public Guid TripId { get; init; }

        public string? Title { get; init; }

        public string DestinationGovernorate { get; init; } = null!;

        [JsonPropertyName("city")]
        public string City => DestinationGovernorate;

        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int DurationDays { get; init; }

        public int People { get; init; }

        [JsonPropertyName("totalBudgetEgp")]
        public int TotalBudgetEgp { get; init; }

        public int TotalCost { get; init; }

        public string Status { get; init; } = null!;
        public string? ShareToken { get; init; }
        public bool IsPublic { get; init; }

        public string? SessionId { get; init; }
        public string? CollectedJson { get; init; }

        public string? CoverImageUrl { get; init; }
        public int PlacesCount { get; init; }
        public int? ProgressPercent { get; init; }

        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        public JsonElement? Plan { get; init; }
    }

    public sealed class TripReviewRequest
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public sealed class TripReviewResponse
    {
        public Guid TripReviewId { get; init; }
        public Guid TripId { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed class MyTripReviewResponse
    {
        public Guid TripReviewId { get; init; }
        public Guid TripId { get; init; }
        public string Destination { get; init; } = null!;
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }
}