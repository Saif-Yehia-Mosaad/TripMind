using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using TripMind.Domain.Enums;

namespace TripMind.Application.DTOs.Trip
{
    public sealed class CreateTripRequest : IValidatableObject
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(100)]
        public string? DestinationGovernorate { get; set; }

        [JsonPropertyName("city")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 50)]
        [JsonPropertyName("people")]
        public int? People { get; set; }

        [Required]
        [Range(1, 100_000_000)]
        [JsonPropertyName("totalBudgetEgp")]
        public int TotalBudgetEgp { get; set; }

        [Range(0, 100_000_000)]
        public int? TotalCost { get; set; }

        public JsonElement? Plan { get; set; }

        public JsonElement? Collected { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public bool? IsPublic { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(DestinationGovernorate) &&
                string.IsNullOrWhiteSpace(City))
            {
                yield return new ValidationResult(
                    "DestinationGovernorate or City is required.",
                    new[] { nameof(DestinationGovernorate), nameof(City) });
            }

            if (EndDate.Date < StartDate.Date)
            {
                yield return new ValidationResult(
                    "EndDate cannot be before StartDate.",
                    new[] { nameof(EndDate) });
            }

            if ((EndDate.Date - StartDate.Date).TotalDays > 60)
            {
                yield return new ValidationResult(
                    "Trip duration cannot exceed 60 days.",
                    new[] { nameof(EndDate) });
            }

            if (TotalCost.HasValue && TotalCost.Value > TotalBudgetEgp)
            {
                yield return new ValidationResult(
                    "TotalCost cannot be greater than TotalBudgetEgp.",
                    new[] { nameof(TotalCost) });
            }
        }
    }



    public sealed class RenameTripRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
    }

    public sealed class UpdateTripRequest
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(100)]
        public string? DestinationGovernorate { get; set; }

        [JsonPropertyName("city")]
        [MaxLength(100)]
        public string? City { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, 50)]
        public int? People { get; set; }

        [JsonPropertyName("totalBudgetEgp")]
        [Range(1, 100_000_000)]
        public int? TotalBudgetEgp { get; set; }

        [Range(0, 100_000_000)]
        public int? TotalCost { get; set; }

        public JsonElement? Plan { get; set; }

        public JsonElement? Collected { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public bool? IsPublic { get; set; }
    }
    public sealed class TripSearchRequest
    {
        public TripStatus? Status { get; set; }

        [RegularExpression("^(updatedAt|createdAt|startDate)$",
            ErrorMessage = "SortBy must be one of: updatedAt, createdAt, startDate.")]
        public string SortBy { get; set; } = "updatedAt";

        [RegularExpression("^(asc|desc)$",
            ErrorMessage = "Order must be asc or desc.")]
        public string Order { get; set; } = "desc";

        [Range(1, 10000, ErrorMessage = "Page must be between 1 and 10000.")]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
    public sealed class PublicTripResponse
    {
        public Guid TripId { get; init; }
        public string? Title { get; init; }
        public string DestinationGovernorate { get; init; } = null!;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int DurationDays { get; init; }
        public int People { get; init; }
        public string Status { get; init; } = null!;
        public string? CoverImageUrl { get; init; }
        public int PlacesCount { get; init; }
        public JsonElement? Plan { get; init; }   // ????? ????? ?????? ??????? ???? Plan ???? ?????
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
