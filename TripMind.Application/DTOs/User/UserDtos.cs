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
        [MaxLength(100)] public string? DisplayName { get; set; }
        [MaxLength(50)] public string? Username { get; set; }
        [MaxLength(20)] public string? PhoneNumber { get; set; }
        [MaxLength(500)] public string? Bio { get; set; }
        [MaxLength(100)] public string? HomeGovernorate { get; set; }
        [MaxLength(2)] public string? LanguagePreference { get; set; }
        [MaxLength(2048)] public string? ProfilePhotoUrl { get; set; }
    }

    public sealed class UpdateInterestsRequest
    {
        [Required] public List<string> Interests { get; set; } = new();
    }
}