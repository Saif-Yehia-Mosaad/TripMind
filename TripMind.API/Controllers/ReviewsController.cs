using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Review;
using TripMind.Application.Services;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/reviews")]
    public sealed class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviews;
        public ReviewsController(ReviewService reviews) => _reviews = reviews;

        private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("location/{locationId:guid}")]
        public async Task<IActionResult> GetByLocation(Guid locationId)
            => Ok(await _reviews.GetLocationReviewsAsync(locationId));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddReviewRequest req)
        {
            var review = await _reviews.AddReviewAsync(Me, req);
            return CreatedAtAction(nameof(GetByLocation), new { locationId = req.LocationId }, review);
        }

        [HttpPost("{id:guid}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote(Guid id, [FromQuery] bool isHelpful)
        {
            await _reviews.VoteAsync(Me, id, isHelpful);
            return Ok();
        }

        [HttpPost("{id:guid}/report")]
        [Authorize]
        public async Task<IActionResult> Report(Guid id)
        {
            await _reviews.ReportReviewAsync(id);
            return Ok();
        }
    }
}
