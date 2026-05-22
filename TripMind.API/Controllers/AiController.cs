using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Ai;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/ai")]
    [Produces("application/json")]
    [Authorize]
    public sealed class AiController : ControllerBase
    {
        private readonly AiService _ai;

        public AiController(AiService ai) => _ai = ai;

        [HttpPost("generate-plan")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GeneratePlan([FromBody] AiSearchRequest req)
        {
            try
            {
                var result = await _ai.GeneratePlanAsync(req);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
        [HttpPost("recommend")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
        {
            try
            {
                var result = await _ai.GetRecommendationsAsync(req);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
        }
    }
}