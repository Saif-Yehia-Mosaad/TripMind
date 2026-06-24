using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Trip;
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

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TripResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] TripSearchRequest req) =>
            Ok(await _trips.GetUserTripsAsync(Me, req));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id) =>
            Ok(await _trips.GetTripByIdAsync(Me, id));

        [HttpPut("{id:guid}/plan")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateTripRequest req)
        {
            try { return Ok(await _trips.UpdatePlanAsync(Me, id, req)); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPatch("{id:guid}/rename")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Rename(Guid id, [FromBody] RenameTripRequest req)
        {
            try { return Ok(await _trips.RenameAsync(Me, id, req)); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] TripStatus status)
        {
            try { return Ok(await _trips.UpdateStatusAsync(Me, id, status)); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPost("{id:guid}/share")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Share(Guid id)
        {
            try
            {
                var token = await _trips.CreateShareLinkAsync(Me, id);
                return Ok(new { shareToken = token });
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet("share/{token}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TripResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetShared(string token)
        {
            try { return Ok(await _trips.GetByShareTokenAsync(token)); }
            catch (KeyNotFoundException) { return NotFound(new { message = "Shared trip not found." }); }
        }

        [HttpPost("{id:guid}/review")]
        [ProducesResponseType(typeof(TripReviewResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddReview(Guid id, [FromBody] TripReviewRequest req)
        {
            try
            {
                var result = await _trips.AddTripReviewAsync(Me, id, req);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Trip not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/review")]
        [ProducesResponseType(typeof(TripReviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] TripReviewRequest req)
        {
            try
            {
                return Ok(await _trips.UpdateTripReviewAsync(Me, id, req));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Review not found." });
            }
        }

        [HttpDelete("{id:guid}/review")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            try
            {
                await _trips.DeleteTripReviewAsync(Me, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Review not found." });
            }
        }

        [HttpGet("{id:guid}/reviews")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TripReviewWithUserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviews(Guid id)
        {
            try
            {
                return Ok(await _trips.GetTripReviewsAsync(id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Trip not found." });
            }
        }

        // أضف ده في TripsController.cs، بعد GetById وقبل UpdatePlan مثلاً

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _trips.DeleteTripAsync(Me, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Trip not found." });
            }
        }

        [HttpGet("{id:guid}/review/me")]
        [ProducesResponseType(typeof(TripReviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReview(Guid id)
        {
            var result = await _trips.GetMyTripReviewAsync(Me, id);
            return result == null ? NotFound() : Ok(result);
        }
    }
}