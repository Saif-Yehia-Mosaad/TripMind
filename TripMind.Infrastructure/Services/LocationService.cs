using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Location;
using TripMind.Domain.Enums;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class LocationService
    {
        private readonly TripMindDbContext _db;
        public LocationService(TripMindDbContext db) => _db = db;

        public async Task<PagedResult<LocationResponse>> SearchAsync(LocationSearchRequest req)
        {
            var query = _db.Locations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Governorate))
                query = query.Where(l => l.Governorate == req.Governorate);

            if (req.Category.HasValue)
                query = query.Where(l => l.Category == req.Category.Value);

            if (req.HiddenGemsOnly == true)
                query = query.Where(l => l.IsHiddenGem);

            if (req.MaxPriceEgp.HasValue)
                query = query.Where(l =>
                    (l.Category == LocationCategory.Hotel && l.AvgPricePerNightEgp <= req.MaxPriceEgp) ||
                    (l.Category == LocationCategory.Restaurant && l.AvgMealPriceEgp <= req.MaxPriceEgp) ||
                    (l.Category == LocationCategory.Park && (l.EntryFeeEgp ?? 0) <= req.MaxPriceEgp));

            if (!string.IsNullOrWhiteSpace(req.Search))
                query = query.Where(l =>
                    l.NameEn.Contains(req.Search) || l.NameAr.Contains(req.Search));

            var total = await query.CountAsync();

            var locations = await query
                .OrderByDescending(l => l.AvgRating)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Include(l => l.HiddenGem)
                .ToListAsync();

            var items = locations.Select(l => new LocationResponse
            {
                LocationId = l.LocationId,
                NameEn = l.NameEn,
                NameAr = l.NameAr,
                Category = l.Category.ToString(),
                Governorate = l.Governorate,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                DescriptionEn = l.DescriptionEn,
                DescriptionAr = l.DescriptionAr,
                EntryFeeEgp = l.EntryFeeEgp,
                AvgPricePerNightEgp = l.AvgPricePerNightEgp,
                AvgMealPriceEgp = l.AvgMealPriceEgp,
                OpeningHours = l.OpeningHours,
                IsHiddenGem = l.IsHiddenGem,
                PopularityScore = l.PopularityScore,
                AvgRating = l.AvgRating,
                HiddenGemStory = l.HiddenGem?.Story
            }).ToList();

            return new PagedResult<LocationResponse>
            {
                Items = items,
                TotalCount = total,
                Page = req.Page,
                PageSize = req.PageSize
            };
        }

        public async Task<LocationResponse> GetByIdAsync(Guid locationId)
        {
            var l = await _db.Locations
                .Include(x => x.HiddenGem)
                .FirstOrDefaultAsync(x => x.LocationId == locationId)
                ?? throw new KeyNotFoundException("Location not found.");

            return new LocationResponse
            {
                LocationId = l.LocationId,
                NameEn = l.NameEn,
                NameAr = l.NameAr,
                Category = l.Category.ToString(),
                Governorate = l.Governorate,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                DescriptionEn = l.DescriptionEn,
                DescriptionAr = l.DescriptionAr,
                EntryFeeEgp = l.EntryFeeEgp,
                AvgPricePerNightEgp = l.AvgPricePerNightEgp,
                AvgMealPriceEgp = l.AvgMealPriceEgp,
                OpeningHours = l.OpeningHours,
                IsHiddenGem = l.IsHiddenGem,
                PopularityScore = l.PopularityScore,
                AvgRating = l.AvgRating,
                HiddenGemStory = l.HiddenGem?.Story
            };
        }

        public async Task<List<LocationResponse>> GetHiddenGemsAsync(string? governorate = null)
        {
            var query = _db.Locations
                .Include(l => l.HiddenGem)
                .Where(l => l.IsHiddenGem);

            if (!string.IsNullOrWhiteSpace(governorate))
                query = query.Where(l => l.Governorate == governorate);

            var gems = await query
                .OrderByDescending(l => l.AvgRating)
                .Take(20)
                .ToListAsync();

            return gems.Select(l => new LocationResponse
            {
                LocationId = l.LocationId,
                NameEn = l.NameEn,
                NameAr = l.NameAr,
                Category = l.Category.ToString(),
                Governorate = l.Governorate,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                IsHiddenGem = true,
                AvgRating = l.AvgRating,
                PopularityScore = l.PopularityScore,
                HiddenGemStory = l.HiddenGem?.Story
            }).ToList();
        }
    }
}