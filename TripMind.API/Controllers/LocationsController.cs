using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Location;
using TripMind.Application.Services;
using TripMind.Infrastructure.Services;
using TripMind.Domain.Enums;

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
    }
}
