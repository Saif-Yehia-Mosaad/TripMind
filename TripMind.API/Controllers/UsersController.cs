using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.User;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public sealed class UsersController : ControllerBase
    {
        private readonly UserService _users;
        public UsersController(UserService users) => _users = users;

        private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        public async Task<IActionResult> GetProfile() => Ok(await _users.GetProfileAsync(Me));

        [HttpPatch("me")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req) =>
            Ok(await _users.UpdateProfileAsync(Me, req));

        [HttpPut("me/interests")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UpdateInterests([FromBody] UpdateInterestsRequest req)
        {
            await _users.UpdateInterestsAsync(Me, req.InterestTags);
            return NoContent();
        }

        [HttpDelete("me")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteAccount()
        {
            await _users.DeleteAccountAsync(Me);
            return NoContent();
        }
    }

    public record UpdateInterestsRequest(List<string> InterestTags);
}
