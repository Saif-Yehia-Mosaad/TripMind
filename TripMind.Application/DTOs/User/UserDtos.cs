using System;
using System.Collections.Generic;

namespace TripMind.Application.DTOs.User
{
    public sealed class UserProfileResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public string? HomeGovernorate { get; set; }
        public string LanguagePreference { get; set; } = "AR";
        public List<string> Interests { get; set; } = new();
    }

    public sealed class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public string? HomeGovernorate { get; set; }
        public string? LanguagePreference { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }
}
