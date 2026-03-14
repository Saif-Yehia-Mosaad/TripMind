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

        [HttpPost("search")]
        [ProducesResponseType(typeof(AiSearchResponse), 200)]
        public async Task<IActionResult> Search([FromBody] AiSearchRequest req) =>
            Ok(await _ai.SearchAsync(req));
    }
}