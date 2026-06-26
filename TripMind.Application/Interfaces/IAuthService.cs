using System;
using System.Threading.Tasks;
using TripMind.Application.DTOs.Auth;

namespace TripMind.Application.Interfaces
{
    public interface IAuthService
    {
        Task<MessageResponse> RegisterAsync(RegisterRequest req, string? ip);

        Task<object> LoginAsync(LoginRequest req, string? ip);

        Task<TokenResponse> VerifyLoginOtpAsync(LoginOtpRequest req, string? ip);

        Task<MessageResponse> ResendTwoFactorOtpAsync(string email, string? ip);

        Task<MessageResponse> VerifyEmailAsync(VerifyEmailOtpRequest req, string? ip);

        Task<MessageResponse> ResendEmailOtpAsync(ResendEmailOtpRequest req, string? ip);
        Task<TokenResponse> GoogleLoginAsync(string idToken, string? ip);

        Task<TokenResponse> FacebookLoginAsync(string accessToken, string? ip);

        Task<TokenResponse> RefreshAsync(string refreshToken, string? ip);

        Task LogoutAsync(Guid userId, string refreshToken);

        Task RevokeAsync(string refreshToken);

        Task SendPasswordResetOtpAsync(ForgotPasswordRequest req, string? ip);

        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest req, string? ip);

        Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest req, string? ip);

        Task<MessageResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest req, string? ip);

        Task<MessageResponse> InitiateTwoFactorAsync(Guid userId, string? ip);

        Task<MessageResponse> ConfirmTwoFactorAsync(Guid userId, TwoFactorConfirmRequest req, string? ip);

        Task<MessageResponse> DisableTwoFactorAsync(Guid userId, TwoFactorDisableRequest req, string? ip);
    }
}
