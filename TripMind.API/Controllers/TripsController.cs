using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TripMind.Application.DTOs.Location;
using TripMind.Application.DTOs.Trip;
using TripMind.Application.Services;
using TripMind.Domain.Enums;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/trips")]
    [Authorize]
    public sealed class TripsController : ControllerBase
    {
        private readonly TripService _trips;

        public TripsController(TripService trips) => _trips = trips;

        private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateTripRequest req)
        {
            var trip = await _trips.CreateTripAsync(Me, req);
            return CreatedAtAction(nameof(GetById), new { id = trip.TripId }, trip);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id) =>
            Ok(await _trips.GetTripByIdAsync(Me, id));

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TripResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] TripSearchRequest req) =>
    Ok(await _trips.GetUserTripsAsync(Me, req));

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTripRequest req) =>
            Ok(await _trips.UpdateTripAsync(Me, id, req));

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] TripStatus status) =>
            Ok(await _trips.UpdateStatusAsync(Me, id, status));

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _trips.DeleteTripAsync(Me, id);
            return NoContent();
        }
    }
}
