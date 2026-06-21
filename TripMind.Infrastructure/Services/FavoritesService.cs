using System;
using System.Collections.Generic;
using System.Linq;
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
        public FavoritesService(IAppDbContext db) => _db = db;

        // ── Places ───────────────────────────────────────────────────────────
        public async Task<FavoritePlaceResponse> AddFavoritePlaceAsync(Guid userId, FavoritePlaceRequest req)
        {
            var existing = await _db.FavoritePlaces
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == req.PlaceId);
            if (existing != null) return Map(existing);

            var fav = new FavoritePlace
            {
                FavoritePlaceId = Guid.NewGuid(),
                UserId = userId,
                PlaceId = req.PlaceId,
                Name = req.Name,
                PhotoUrl = req.PhotoUrl,
                CityEn = req.CityEn,
                Category = req.Category,
                Rating = req.Rating,
                CreatedAt = DateTime.UtcNow
            };
            _db.FavoritePlaces.Add(fav);
            await _db.SaveChangesAsync();
            return Map(fav);
        }

        public async Task RemoveFavoritePlaceAsync(Guid userId, string placeId)
        {
            var fav = await _db.FavoritePlaces
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId)
                ?? throw new KeyNotFoundException("Favorite place not found.");
            _db.FavoritePlaces.Remove(fav);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FavoritePlaceResponse>> GetFavoritePlacesAsync(Guid userId) =>
            await _db.FavoritePlaces
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => Map(f))
                .ToListAsync();

        // ── Trips ────────────────────────────────────────────────────────────
        public async Task<FavoriteTripResponse> AddFavoriteTripAsync(Guid userId, Guid tripId)
        {
            var tripExists = await _db.Trips.AnyAsync(t => t.TripId == tripId && t.UserId == userId);
            if (!tripExists) throw new KeyNotFoundException("Trip not found.");

            var existing = await _db.FavoriteTrips
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TripId == tripId);
            if (existing != null) return MapTrip(existing);

            var fav = new FavoriteTrip
            {
                FavoriteTripId = Guid.NewGuid(),
                UserId = userId,
                TripId = tripId,
                CreatedAt = DateTime.UtcNow
            };
            _db.FavoriteTrips.Add(fav);
            await _db.SaveChangesAsync();
            return MapTrip(fav);
        }

        public async Task RemoveFavoriteTripAsync(Guid userId, Guid tripId)
        {
            var fav = await _db.FavoriteTrips
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TripId == tripId)
                ?? throw new KeyNotFoundException("Favorite trip not found.");
            _db.FavoriteTrips.Remove(fav);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FavoriteTripResponse>> GetFavoriteTripsAsync(Guid userId) =>
            await _db.FavoriteTrips
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => MapTrip(f))
                .ToListAsync();

        private static FavoritePlaceResponse Map(FavoritePlace f) => new()
        {
            FavoritePlaceId = f.FavoritePlaceId,
            PlaceId = f.PlaceId,
            Name = f.Name,
            PhotoUrl = f.PhotoUrl,
            CityEn = f.CityEn,
            Category = f.Category,
            Rating = f.Rating,
            CreatedAt = f.CreatedAt
        };

        private static FavoriteTripResponse MapTrip(FavoriteTrip f) => new()
        {
            FavoriteTripId = f.FavoriteTripId,
            TripId = f.TripId,
            CreatedAt = f.CreatedAt
        };
    }
}