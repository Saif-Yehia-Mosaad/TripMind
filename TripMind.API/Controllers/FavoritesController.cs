using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Favorite;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/favorites")]
    [Authorize]
    public sealed class FavoritesController : ControllerBase
    {
        private readonly FavoritesService _favorites;
        public FavoritesController(FavoritesService favorites) => _favorites = favorites;

        [HttpGet("places")]
        public async Task<IActionResult> GetPlaces() => Ok(await _favorites.GetFavoritePlacesAsync(Me()));

        [HttpPost("places")]
        public async Task<IActionResult> AddPlace([FromBody] FavoritePlaceRequest req) =>
            Ok(await _favorites.AddFavoritePlaceAsync(Me(), req));

        [HttpDelete("places/{placeId}")]
        public async Task<IActionResult> RemovePlace(string placeId)
        {
            try { await _favorites.RemoveFavoritePlaceAsync(Me(), placeId); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpGet("trips")]
        public async Task<IActionResult> GetTrips() => Ok(await _favorites.GetFavoriteTripsAsync(Me()));

        [HttpPost("trips/{tripId:guid}")]
        public async Task<IActionResult> AddTrip(Guid tripId)
        {
            try { return Ok(await _favorites.AddFavoriteTripAsync(Me(), tripId)); }
            catch (KeyNotFoundException) { return NotFound(new { message = "Trip not found." }); }
        }

        [HttpDelete("trips/{tripId:guid}")]
        public async Task<IActionResult> RemoveTrip(Guid tripId)
        {
            try { await _favorites.RemoveFavoriteTripAsync(Me(), tripId); return NoContent(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        private Guid Me() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}