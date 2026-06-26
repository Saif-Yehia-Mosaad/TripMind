using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripMind.Application.DTOs.User;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class UserService : IUserService
    {
        private readonly TripMindDbContext _db;
        public UserService(TripMindDbContext db) => _db = db;

        public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
        {
            var user = await _db.Users
                .Include(u => u.UserInterests)
                .FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");

            return MapToResponse(user);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (req.Username != null && req.Username != user.Username)
            {
                bool taken = await _db.Users.AnyAsync(u => u.Username == req.Username && u.UserId != userId);
                if (taken) throw new InvalidOperationException("Username is already taken.");
            }

            if (req.DisplayName != null) user.DisplayName = req.DisplayName.Trim();
            if (req.Username != null) user.Username = req.Username.Trim();
            if (req.PhoneNumber != null) user.PhoneNumber = req.PhoneNumber.Trim();
            if (req.Bio != null) user.Bio = req.Bio.Trim();
            if (req.HomeGovernorate != null) user.HomeGovernorate = req.HomeGovernorate;
            if (req.LanguagePreference != null) user.LanguagePreference = req.LanguagePreference;
            if (req.ProfilePhotoUrl != null) user.ProfilePhotoUrl = req.ProfilePhotoUrl;

            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        public async Task<UserDashboardResponse> GetDashboardAsync(Guid userId)
        {
            var trips = await _db.Trips.CountAsync(t => t.UserId == userId);
            var reviews = await _db.TripReviews.CountAsync(r => r.UserId == userId);
            var savedPlaces = await _db.FavoritePlaces.CountAsync(f => f.UserId == userId);
            var savedTrips = await _db.FavoriteTrips.CountAsync(f => f.UserId == userId);

            return new UserDashboardResponse
            {
                TotalTrips = trips,
                TotalReviews = reviews,
                TotalSaved = savedPlaces + savedTrips
            };
        }

        public async Task UpdateInterestsAsync(Guid userId, List<string> interests)
        {
            var existing = await _db.UserInterests.Where(i => i.UserId == userId).ToListAsync();
            _db.UserInterests.RemoveRange(existing);

            foreach (var tag in interests.Distinct())
            {
                _db.UserInterests.Add(new UserInterest
                {
                    UserInterestId = Guid.NewGuid(),
                    UserId = userId,
                    InterestTag = tag,
                    Weight = 1
                });
            }

            await _db.SaveChangesAsync();
        }

        // UserService.DeleteAccountAsync
        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");

            try
            {
                var favoritePlaces = await _db.FavoritePlaces
                    .Where(f => f.UserId == userId)
                    .ToListAsync();
                _db.FavoritePlaces.RemoveRange(favoritePlaces);

                var favoriteTrips = await _db.FavoriteTrips
                    .Where(f => f.UserId == userId)
                    .ToListAsync();
                _db.FavoriteTrips.RemoveRange(favoriteTrips);

                var tripReviews = await _db.TripReviews
                    .Where(r => r.UserId == userId)
                    .ToListAsync();
                _db.TripReviews.RemoveRange(tripReviews);

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "Could not delete account due to related data. Please contact support.", ex);
            }
        }
        private static UserProfileResponse MapToResponse(User u) => new()
        {
            UserId = u.UserId,
            DisplayName = u.DisplayName,
            Username = u.Username,
            PhoneNumber = u.PhoneNumber,
            Bio = u.Bio,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            HomeGovernorate = u.HomeGovernorate,
            LanguagePreference = u.LanguagePreference,
            IsEmailVerified = u.IsEmailVerified,
            TwoFactorEnabled = u.TwoFactorEnabled,
            Interests = u.UserInterests.Select(i => i.InterestTag).ToList()
        };
    }
}