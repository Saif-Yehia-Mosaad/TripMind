using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.User;
using TripMind.Domain.Entities;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class UserService
    {
        private readonly TripMindDbContext _db;
        public UserService(TripMindDbContext db) => _db = db;

        public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
        {
            var user = await _db.Users
                .Include(u => u.UserInterests)
                .Include(u => u.UserPreferences)
                .FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new KeyNotFoundException("User not found.");
            return Map(user);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");
            if (req.DisplayName        != null) user.DisplayName        = req.DisplayName.Trim();
            if (req.HomeGovernorate    != null) user.HomeGovernorate    = req.HomeGovernorate;
            if (req.LanguagePreference != null) user.LanguagePreference = req.LanguagePreference;
            if (req.ProfilePhotoUrl    != null) user.ProfilePhotoUrl    = req.ProfilePhotoUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await GetProfileAsync(userId);
        }

        public async Task UpdateInterestsAsync(Guid userId, List<string> tags)
        {
            var existing = _db.UserInterests.Where(i => i.UserId == userId);
            _db.UserInterests.RemoveRange(existing);
            foreach (var tag in tags)
                _db.UserInterests.Add(new UserInterest
                {
                    UserInterestId = Guid.NewGuid(),
                    UserId         = userId,
                    InterestTag    = tag
                });
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        private static UserProfileResponse Map(User u) => new()
        {
            UserId             = u.UserId,
            Email              = u.Email,
            DisplayName        = u.DisplayName,
            ProfilePhotoUrl    = u.ProfilePhotoUrl,
            HomeGovernorate    = u.HomeGovernorate,
            LanguagePreference = u.LanguagePreference,
            Interests          = u.UserInterests.Select(i => i.InterestTag).ToList()
        };
    }
}
