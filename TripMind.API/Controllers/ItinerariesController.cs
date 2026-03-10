using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Itinerary;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/itineraries")]
    [Authorize]
    public sealed class ItinerariesController : ControllerBase
    {
        private readonly ItineraryService _svc;
        public ItinerariesController(ItineraryService svc) => _svc = svc;

        private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetSaved() => Ok(await _svc.GetSavedAsync(Me));

        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<IActionResult> Save([FromBody] SaveItineraryRequest req)
        {
            var saved = await _svc.SaveAsync(Me, req);
            return StatusCode(201, saved);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.DeleteAsync(Me, id);
            return NoContent();
        }

        [HttpGet("share/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByShareToken(string token) =>
            Ok(await _svc.GetByShareTokenAsync(token));
    }
}
