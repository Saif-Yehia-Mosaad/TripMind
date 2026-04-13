using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TripMind.Application.DTOs.Location;
using TripMind.Application.Services;
using TripMind.Domain.Enums;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/locations")]
    public sealed class LocationsController : ControllerBase
    {
        private readonly LocationService _locations;
        public LocationsController(LocationService locations) => _locations = locations;

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] LocationSearchRequest req)
            => Ok(await _locations.SearchAsync(req));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await _locations.GetByIdAsync(id));

        [HttpGet("hidden-gems")]
        public async Task<IActionResult> GetHiddenGems([FromQuery] string? governorate = null)
            => Ok(await _locations.GetHiddenGemsAsync(governorate));

        private Guid Me() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("recommended")]
        [Authorize]
        [ProducesResponseType(typeof(List<LocationResponse>), 200)]
        public async Task<IActionResult> GetRecommended([FromQuery] int count = 10) =>
    Ok(await _locations.GetRecommendedAsync(Me(), count));

        [HttpGet("popular")]
        [ProducesResponseType(typeof(List<LocationResponse>), 200)]
        public async Task<IActionResult> GetPopular([FromQuery] string? governorate = null, [FromQuery] int count = 20) =>
            Ok(await _locations.GetPopularAsync(governorate, count));
    }
}
