using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.User;
using TripMind.Application.Interfaces;
using TripMind.Infrastructure.Services;
using System.Collections.Generic;
using System.Linq;

namespace TripMind.API.Controllers
{
    public record UploadPhotoResponse(string Url);

    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    [Produces("application/json")]
    public sealed class UsersController : ControllerBase
    {
        private readonly UserService _users;
        private readonly IImageService _images;

        public UsersController(UserService users, IImageService images)
        {
            _users = users;
            _images = images;
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
            var user = await _users.GetProfileAsync(Me());
            await _images.DeleteAsync(user.ProfilePhotoUrl);

            string url;
            try
            {
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

        private Guid Me()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null) throw new UnauthorizedAccessException();
            return Guid.Parse(id);
        }
    }
}