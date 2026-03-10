using System;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Auth
{
    public sealed class RegisterRequest
    {
        [Required][MaxLength(100)] public string DisplayName { get; set; } = null!;
        [Required][EmailAddress][MaxLength(256)] public string Email { get; set; } = null!;
        [Required][MinLength(8)] public string Password { get; set; } = null!;
        [Required][Compare(nameof(Password))] public string ConfirmPassword { get; set; } = null!;
        public bool RememberMe { get; set; }
    }

    public sealed class LoginRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }

    public sealed class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = null!;
    }

    public sealed class ForgotPasswordRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
    }

    public sealed class VerifyOtpRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required][StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$")] public string Otp { get; set; } = null!;
    }

    public sealed class ResetPasswordRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required] public string ResetToken { get; set; } = null!;
        [Required][MinLength(8)] public string NewPassword { get; set; } = null!;
        [Required][Compare(nameof(NewPassword))] public string ConfirmNewPassword { get; set; } = null!;
    }

    public sealed class TokenResponse
    {
        public string AccessToken { get; init; } = null!;
        public string TokenType { get; init; } = "Bearer";
        public int ExpiresIn { get; init; }
        public string RefreshToken { get; init; } = null!;
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? ProfilePhotoUrl { get; init; }
        public string LanguagePreference { get; init; } = "AR";
    }

    public sealed class VerifyOtpResponse
    {
        public string ResetToken { get; init; } = null!;
        public string Message { get; init; } = "OTP verified. You may now reset your password.";
    }

    public sealed class ResetPasswordResponse
    {
        public string Message { get; init; } = "Your password has been successfully updated.";
    }
}
