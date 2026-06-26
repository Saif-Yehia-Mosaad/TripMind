using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TripMind.Application.DTOs.User;
using TripMind.Application.Interfaces;
using TripMind.API.Extensions;

namespace TripMind.API.Controllers
{
    public record UploadPhotoResponse(string Url);

    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    [Produces("application/json")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly ITripService _trips;
        private readonly IImageService _images;


        public UsersController(
        IUserService users,
        IImageService images,
        ITripService trips)
        {
            _users = users;
            _images = images;
            _trips = trips;
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        public async Task<IActionResult> GetProfile() =>
            Ok(await _users.GetProfileAsync(Me()));

        [HttpPatch("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req) =>
            Ok(await _users.UpdateProfileAsync(Me(), req));

        [HttpPost("me/photo")]
        [ProducesResponseType(typeof(UploadPhotoResponse), 200)]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Image file is required." });
            var user = await _users.GetProfileAsync(Me());

            string url;
            try
            {
                // ?) ??? ?????? ??????? ?????
                url = await _images.UploadProfilePhotoAsync(file);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

            // ?) ????? ????????? ??????? ??????
            await _users.UpdateProfileAsync(Me(), new UpdateProfileRequest { ProfilePhotoUrl = url });

            // ?) ??? ?????? ??????? ??? ??? ???? ?? ???? ???
            if (!string.IsNullOrEmpty(user.ProfilePhotoUrl) && user.ProfilePhotoUrl != url)
            {
                try { await _images.DeleteAsync(user.ProfilePhotoUrl); }
                catch { /* ????? ???? ??? ?? ??? ? ?????? ??????? ????? orphaned ?????? ?? batch job ?????? */ }
            }

            return Ok(new UploadPhotoResponse(url));
        }

        [HttpGet("me/dashboard")]
        [ProducesResponseType(typeof(UserDashboardResponse), 200)]
        public async Task<IActionResult> GetDashboard() =>
            Ok(await _users.GetDashboardAsync(Me()));

        [HttpDelete("me")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteAccount()
        {
            await _users.DeleteAccountAsync(Me());
            return NoContent();
        }

        [HttpPut("me/interests")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<IActionResult> UpdateInterests([FromBody] UpdateInterestsRequest req)
        {
            await _users.UpdateInterestsAsync(Me(), req.Interests);
            return Ok(req.Interests.Distinct().ToList());
        }

        [HttpGet("me/reviews")]
        public async Task<IActionResult> GetMyReviews() => Ok(await _trips.GetMyReviewsAsync(Me()));

        private Guid Me() => User.GetUserId();
    }
}
