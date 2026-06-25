using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.User
{
    public sealed class UserProfileResponse
    {
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = null!;
        public string? Username { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Bio { get; init; }
        public string Email { get; init; } = null!;
        public string? ProfilePhotoUrl { get; init; }
        public string? HomeGovernorate { get; init; }
        public string LanguagePreference { get; init; } = "AR";
        public bool IsEmailVerified { get; init; }
        public bool TwoFactorEnabled { get; init; }
        public List<string> Interests { get; init; } = new();
    }

    public sealed class UserDashboardResponse
    {
        public int TotalTrips { get; init; }
        public int TotalReviews { get; init; }
        public int TotalSaved { get; init; }
    }

    public sealed class UpdateProfileRequest
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(50)]
        [RegularExpression(
            "^[a-zA-Z0-9_]{3,50}$",
            ErrorMessage = "Username must be 3-50 characters: letters, numbers, underscore only.")]
        public string? Username { get; set; }

        [MaxLength(20)]
        [RegularExpression(
            @"^\+?[0-9]{8,15}$",
            ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? HomeGovernorate { get; set; }

        [MaxLength(2)]
        public string? LanguagePreference { get; set; }

        [MaxLength(2048)]
        public string? ProfilePhotoUrl { get; set; }
    }

    public sealed class UpdateInterestsRequest : IValidatableObject
    {
        [Required]
        public List<string> Interests { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Interests.Count > 20)
            {
                yield return new ValidationResult(
                    "Cannot have more than 20 interests.",
                    new[] { nameof(Interests) });
            }
        }
    }
}