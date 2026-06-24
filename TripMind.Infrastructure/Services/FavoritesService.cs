using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Favorite;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Services
{
    public sealed class FavoritesService
    {
        private readonly IAppDbContext _db;
        private readonly AiService _ai;

        public FavoritesService(IAppDbContext db, AiService ai)
        {
            _db = db;
            _ai = ai;
        }

        public async Task<FavoritePlaceResponse> AddFavoritePlaceAsync(Guid userId, FavoritePlaceRequest req)
        {
            var place = await _ai.GetPlaceByIdAsync(req.PlaceId);

            var existing = await _db.FavoritePlaces
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == req.PlaceId);

            if (existing != null)
                return Map(existing, place);

            var fav = new FavoritePlace
            {
                FavoritePlaceId = Guid.NewGuid(),
                UserId = userId,
                PlaceId = req.PlaceId,
                CreatedAt = DateTime.UtcNow
            };

            _db.FavoritePlaces.Add(fav);
            await _db.SaveChangesAsync();

            return Map(fav, place);
        }

        public async Task RemoveFavoritePlaceAsync(Guid userId, string placeId)
        {
            var fav = await _db.FavoritePlaces
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId)
                ?? throw new KeyNotFoundException("Favorite place not found.");

            _db.FavoritePlaces.Remove(fav);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FavoritePlaceResponse>> GetFavoritePlacesAsync(Guid userId)
        {
            var favorites = await _db.FavoritePlaces
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // بدل sequential await في loop (N requests متتالية)، بقت parallel
            var tasks = favorites.Select(async fav =>
            {
                try
                {
                    var place = await _ai.GetPlaceByIdAsync(fav.PlaceId);
                    return Map(fav, place);
                }
                catch (KeyNotFoundException)
                {
                    // المكان ممكن يكون مسحوه من مصدر الـ AI الخارجي
                    // - بدل ما نكسّر الليستة كلها، نتجاهله بس
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList()!;
        }

        public async Task<FavoriteTripResponse> AddFavoriteTripAsync(Guid userId, Guid tripId)
        {
            var trip = await _db.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            var existing = await _db.FavoriteTrips
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TripId == tripId);

            if (existing != null)
                return MapTrip(existing, trip);

            var fav = new FavoriteTrip
            {
                FavoriteTripId = Guid.NewGuid(),
                UserId = userId,
                TripId = tripId,
                CreatedAt = DateTime.UtcNow
            };

            _db.FavoriteTrips.Add(fav);
            await _db.SaveChangesAsync();

            return MapTrip(fav, trip);
        }

        public async Task RemoveFavoriteTripAsync(Guid userId, Guid tripId)
        {
            var fav = await _db.FavoriteTrips
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TripId == tripId)
                ?? throw new KeyNotFoundException("Favorite trip not found.");

            _db.FavoriteTrips.Remove(fav);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FavoriteTripResponse>> GetFavoriteTripsAsync(Guid userId)
        {
            var favorites = await _db.FavoriteTrips
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var tripIds = favorites.Select(x => x.TripId).ToList();

            var trips = await _db.Trips
                .Where(t => tripIds.Contains(t.TripId))
                .ToDictionaryAsync(t => t.TripId);

            return favorites
                .Select(f => MapTrip(f, trips.TryGetValue(f.TripId, out var trip) ? trip : null))
                .ToList();
        }

        private static FavoritePlaceResponse Map(FavoritePlace f, JsonElement place) => new()
        {
            FavoritePlaceId = f.FavoritePlaceId,
            PlaceId = f.PlaceId,
            Place = place,
            CreatedAt = f.CreatedAt
        };

        private static FavoriteTripResponse MapTrip(FavoriteTrip f, Trip? trip) => new()
        {
            FavoriteTripId = f.FavoriteTripId,
            TripId = f.TripId,
            Destination = trip?.DestinationGovernorate ?? string.Empty,
            StartDate = trip?.StartDate ?? default,
            EndDate = trip?.EndDate ?? default,
            DurationDays = trip?.DurationDays ?? 0,
            Status = trip?.Status.ToString() ?? string.Empty,
            CreatedAt = f.CreatedAt
        };
    }
}