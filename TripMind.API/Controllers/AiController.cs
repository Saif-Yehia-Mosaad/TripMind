using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Ai;
using TripMind.Application.Interfaces;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/ai")]
    [Produces("application/json")]
    [Authorize]
    public sealed class AiController : ControllerBase
    {
        private readonly IAiService _ai;
        private readonly IWebHostEnvironment _env;

        public AiController(
            IAiService ai,
            IWebHostEnvironment env)
        {
            _ai = ai;
            _env = env;
        }

        [HttpPost("generate-plan")]
        public async Task<IActionResult> GeneratePlan([FromBody] GeneratePlanRequest req)
            => await Run(() => _ai.GeneratePlanAsync(req));

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
            => await Run(() => _ai.ChatAsync(req));

        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromBody] EditRequest req)
            => await Run(() => _ai.EditAsync(req));

        [HttpPost("places/home")]
        public async Task<IActionResult> Home([FromBody] HomeRequest req)
            => await Run(() => _ai.HomeAsync(req));

        [HttpPost("places/recommend")]
        public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
            => await Run(() => _ai.RecommendAsync(req));

        [HttpPost("places/search")]
        public async Task<IActionResult> SearchPlaces([FromBody] SearchPlacesRequest req)
            => await Run(() => _ai.SearchPlacesAsync(req));

        [HttpPost("places/nearby")]
        public async Task<IActionResult> Nearby([FromBody] NearbyRequest req)
            => await Run(() => _ai.NearbyAsync(req));

        [HttpPost("places/top-rated")]
        public async Task<IActionResult> TopRated([FromBody] TopRatedRequest req)
            => await Run(() => _ai.TopRatedAsync(req));

        [HttpPost("places/getplaces")]
        public async Task<IActionResult> GetPlaces([FromBody] GetPlacesRequest req)
            => await Run(() => _ai.GetPlacesAsync(req));

        [HttpGet("places/{placeId}")]
        public async Task<IActionResult> GetPlace(string placeId)
        {
            try
            {
                return Ok(await _ai.GetPlaceByIdAsync(placeId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Place not found." });
            }
            catch (AiServiceException ex)
            {
                var detail = _env.IsDevelopment() ? ex.RawBody : null;
                return StatusCode(ex.StatusCode, new { message = ex.Message, detail });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }

        private async Task<IActionResult> Run(Func<Task<JsonElement>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (AiServiceException ex)
            {
                var detail = _env.IsDevelopment() ? ex.RawBody : null;
                return StatusCode(ex.StatusCode, new { message = ex.Message, detail });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
    }
}