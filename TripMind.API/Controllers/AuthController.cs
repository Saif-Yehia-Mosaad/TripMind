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
        [ProducesResponseType(typeof(TokenResponse), 201)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _auth.RegisterAsync(req, Ip());
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req) =>
            Ok(await _auth.LoginAsync(req, Ip()));

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

        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            await _auth.SendPasswordResetOtpAsync(req, Ip());
            return Ok(new { message = "If that email is registered, a verification code has been sent." });
        }

        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(VerifyOtpResponse), 200)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req) =>
            Ok(await _auth.VerifyOtpAsync(req, Ip()));

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ResetPasswordResponse), 200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req) =>
            Ok(await _auth.ResetPasswordAsync(req, Ip()));

        private Guid Me() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string? Ip() =>
            HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd)
                ? fwd.ToString().Split(',')[0].Trim()
                : HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}