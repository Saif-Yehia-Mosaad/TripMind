using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Auth;
using TripMind.Application.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public sealed class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth) => _auth = auth;

        [HttpPost("register")]
        [ProducesResponseType(typeof(MessageResponse), 201)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _auth.RegisterAsync(req, Ip());
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(typeof(PendingTwoFactorResponse), 200)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req) =>
            Ok(await _auth.LoginAsync(req, Ip()));

        [HttpPost("login/verify")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] LoginOtpRequest req) =>
            Ok(await _auth.VerifyLoginOtpAsync(req, Ip()));

        [HttpPost("login/resend-otp")]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ResendLoginOtp([FromBody] ResendEmailOtpRequest req) =>
            Ok(await _auth.ResendTwoFactorOtpAsync(req.Email, Ip()));

        [HttpPost("email/verify")]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailOtpRequest req) =>
            Ok(await _auth.VerifyEmailAsync(req, Ip()));

        [HttpPost("email/resend-otp")]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ResendEmailOtp([FromBody] ResendEmailOtpRequest req) =>
            Ok(await _auth.ResendEmailOtpAsync(req, Ip()));

        [HttpPost("google")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req) =>
            Ok(await _auth.GoogleLoginAsync(req.IdToken, Ip()));

        [HttpPost("facebook")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest req) =>
            Ok(await _auth.FacebookLoginAsync(req.AccessToken, Ip()));

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req) =>
            Ok(await _auth.RefreshAsync(req.RefreshToken, Ip()));

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
        {
            await _auth.LogoutAsync(Me(), req.RefreshToken);
            return NoContent();
        }

        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest req)
        {
            await _auth.RevokeAsync(req.RefreshToken);
            return NoContent();
        }

        [HttpPost("password/forgot")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            await _auth.SendPasswordResetOtpAsync(req, Ip());
            return Ok(new { message = "If that email is registered, a verification code has been sent." });
        }

        [HttpPost("password/verifyotp")]
        [ProducesResponseType(typeof(VerifyOtpResponse), 200)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req) =>
            Ok(await _auth.VerifyOtpAsync(req, Ip()));

        [HttpPost("password/reset")]
        [ProducesResponseType(typeof(ResetPasswordResponse), 200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req) =>
            Ok(await _auth.ResetPasswordAsync(req, Ip()));

        [HttpPost("password/resend-otp")]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ResendPasswordOtp([FromBody] ForgotPasswordRequest req)
        {
            await _auth.SendPasswordResetOtpAsync(req, Ip());
            return Ok(new MessageResponse { Message = "If that email is registered, a new OTP has been sent." });
        }

        [HttpPost("password/change")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req) =>
            Ok(await _auth.ChangePasswordAsync(Me(), req, Ip()));

        [HttpPost("2fa/initiate")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> InitiateTwoFactor() =>
            Ok(await _auth.InitiateTwoFactorAsync(Me(), Ip()));

        [HttpPost("2fa/confirm")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ConfirmTwoFactor([FromBody] TwoFactorConfirmRequest req) =>
            Ok(await _auth.ConfirmTwoFactorAsync(Me(), req, Ip()));

        [HttpPost("2fa/disable")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> DisableTwoFactor([FromBody] TwoFactorDisableRequest req) =>
            Ok(await _auth.DisableTwoFactorAsync(Me(), req, Ip()));

        [HttpPost("2fa/resend-otp")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> ResendTwoFactorOtp() =>
            Ok(await _auth.ResendTwoFactorOtpAsync(User.FindFirstValue(ClaimTypes.Email)!, Ip()));

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult Me2() => Ok(new
        {
            UserId = Me(),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Name = User.FindFirstValue(ClaimTypes.Name)
        });

        private Guid Me() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string? Ip() =>
            HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd)
                ? fwd.ToString().Split(',')[0].Trim()
                : HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}