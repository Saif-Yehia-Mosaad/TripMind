using System;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Auth
{
    public sealed class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(
    @"^[^<>{}]*$",
    ErrorMessage = "DisplayName cannot contain HTML special characters.")]
        public string DisplayName { get; set; } = null!;
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [RegularExpression(@"^[^@]+@[^@]+\.[^@]{2,}$", ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password))]
        [MinLength(8)]
        [MaxLength(128)]
        public string ConfirmPassword { get; set; } = null!;
        public bool RememberMe { get; set; }
    }

    public sealed class LoginRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }

    public sealed class VerifyEmailOtpRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required][StringLength(6, MinimumLength = 6)] public string Otp { get; set; } = null!;
    }

    public sealed class ResendEmailOtpRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
    }

    public sealed class LoginOtpRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required][StringLength(6, MinimumLength = 6)] public string Otp { get; set; } = null!;
    }

    public sealed class ChangePasswordRequest
    {
        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword))]
        [MinLength(8)]
        [MaxLength(128)]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public sealed class TwoFactorInitiateRequest { }

    public sealed class TwoFactorConfirmRequest
    {
        [Required][StringLength(6, MinimumLength = 6)] public string Otp { get; set; } = null!;
    }

    public sealed class TwoFactorDisableRequest
    {
        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = null!;
    }

    public sealed class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = null!;
    }

    public sealed class LogoutRequest
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
        [Required]
        [StringLength(6, MinimumLength = 4)]
        [RegularExpression(@"^\d{4,6}$")] public string Otp { get; set; } = null!;
    }

    public sealed class GoogleLoginRequest
    {
        [Required] public string IdToken { get; set; } = null!;
    }

    public sealed class FacebookLoginRequest
    {
        [Required] public string AccessToken { get; set; } = null!;
    }

    public sealed class ResetPasswordRequest
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required] public string ResetToken { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword))]
        [MinLength(8)]
        [MaxLength(128)]
        public string ConfirmNewPassword { get; set; } = null!;
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
        public bool IsEmailVerified { get; init; }
        public bool TwoFactorEnabled { get; init; }
    }

    public sealed class PendingTwoFactorResponse
    {
        public string Message { get; init; } = "OTP sent to your email. Please verify to complete login.";
        public string Email { get; init; } = null!;
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

    public sealed class MessageResponse
    {
        public string Message { get; init; } = null!;
    }
}
