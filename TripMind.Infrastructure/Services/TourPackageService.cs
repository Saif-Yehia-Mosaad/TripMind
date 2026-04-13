using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Location;
using TripMind.Application.DTOs.TourPackage;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class TourPackageService
    {
        private readonly TripMindDbContext _db;
        public TourPackageService(TripMindDbContext db) => _db = db;

        public async Task<PagedResult<TourPackageResponse>> SearchAsync(TourPackageSearchRequest req)
        {
            var query = _db.TourPackages.Where(t => t.IsActive);

            if (!string.IsNullOrWhiteSpace(req.Governorate))
                query = query.Where(t => t.Governorate == req.Governorate);

            if (req.MinDays.HasValue)
                query = query.Where(t => t.DurationDays >= req.MinDays.Value);

            if (req.MaxDays.HasValue)
                query = query.Where(t => t.DurationDays <= req.MaxDays.Value);

            if (req.MaxPricePerPerson.HasValue)
                query = query.Where(t => t.PricePerPersonEgp <= req.MaxPricePerPerson.Value);

            var total = await query.CountAsync();

            var packages = await query
                .OrderByDescending(t => t.AvgRating)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Include(t => t.Locations)
                    .ThenInclude(l => l.Location)
                .ToListAsync();

            var items = packages.Select(p => new TourPackageResponse
            {
                TourPackageId = p.TourPackageId,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                Governorate = p.Governorate,
                DurationDays = p.DurationDays,
                PricePerPersonEgp = p.PricePerPersonEgp,
                PhotoUrl = p.PhotoUrl,
                AvgRating = p.AvgRating,
                Locations = p.Locations
                    .OrderBy(l => l.DayNumber).ThenBy(l => l.SequenceOrder)
                    .Select(l => new TourPackageLocationResponse
                    {
                        LocationId = l.LocationId,
                        NameEn = l.Location.NameEn,
                        NameAr = l.Location.NameAr,
                        Category = l.Location.Category.ToString(),
                        DayNumber = l.DayNumber,
                        SequenceOrder = l.SequenceOrder,
                        Latitude = l.Location.Latitude,
                        Longitude = l.Location.Longitude
                    }).ToList()
            }).ToList();

            return new PagedResult<TourPackageResponse>
            {
                Items = items,
                TotalCount = total,
                Page = req.Page,
                PageSize = req.PageSize
            };
        }

        public async Task<TourPackageResponse> GetByIdAsync(Guid id)
        {
            var p = await _db.TourPackages
                .Include(t => t.Locations).ThenInclude(l => l.Location)
                .FirstOrDefaultAsync(t => t.TourPackageId == id && t.IsActive)
                ?? throw new KeyNotFoundException("Tour package not found.");

            return new TourPackageResponse
            {
                TourPackageId = p.TourPackageId,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                Governorate = p.Governorate,
                DurationDays = p.DurationDays,
                PricePerPersonEgp = p.PricePerPersonEgp,
                PhotoUrl = p.PhotoUrl,
                AvgRating = p.AvgRating,
                Locations = p.Locations
                    .OrderBy(l => l.DayNumber).ThenBy(l => l.SequenceOrder)
                    .Select(l => new TourPackageLocationResponse
                    {
                        LocationId = l.LocationId,
                        NameEn = l.Location.NameEn,
                        NameAr = l.Location.NameAr,
                        Category = l.Location.Category.ToString(),
                        DayNumber = l.DayNumber,
                        SequenceOrder = l.SequenceOrder,
                        Latitude = l.Location.Latitude,
                        Longitude = l.Location.Longitude
                    }).ToList()
            };
        }
    }
}