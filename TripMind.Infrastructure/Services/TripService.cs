using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TripMind.Application.DTOs.Location;
using TripMind.Application.DTOs.Trip;
using TripMind.Domain.Entities;
using TripMind.Domain.Enums;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class TripService
    {
        private readonly TripMindDbContext _db;

        public TripService(TripMindDbContext db) => _db = db;

        public async Task<TripResponse> CreateTripAsync(Guid userId, CreateTripRequest req)
        {
            if (req.EndDate <= req.StartDate)
                throw new InvalidOperationException("End date must be after start date.");

            int duration = (req.EndDate - req.StartDate).Days + 1;

            var trip = new Trip
            {
                TripId                 = Guid.NewGuid(),
                UserId                 = userId,
                DestinationGovernorate = req.DestinationGovernorate,
                StartDate              = req.StartDate,
                EndDate                = req.EndDate,
                DurationDays           = duration,
                TotalBudgetEgp         = req.TotalBudgetEgp,
                Status                 = TripStatus.Draft,
                IsPublic               = false,
                ShareToken             = GenerateShareToken(),
                CreatedAt              = DateTime.UtcNow
            };

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync();

            return await GetTripByIdAsync(userId, trip.TripId);
        }

        public async Task<TripResponse> UpdateTripAsync(Guid userId, Guid tripId, UpdateTripRequest req)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                throw new InvalidOperationException("Cannot edit a completed or cancelled trip.");

            if (req.DestinationGovernorate != null) trip.DestinationGovernorate = req.DestinationGovernorate;
            if (req.StartDate.HasValue)             trip.StartDate              = req.StartDate.Value;
            if (req.EndDate.HasValue)               trip.EndDate                = req.EndDate.Value;
            if (req.TotalBudgetEgp.HasValue)        trip.TotalBudgetEgp         = req.TotalBudgetEgp.Value;
            if (req.IsPublic.HasValue)              trip.IsPublic               = req.IsPublic.Value;

            if (req.StartDate.HasValue || req.EndDate.HasValue)
                trip.DurationDays = (trip.EndDate - trip.StartDate).Days + 1;

            await _db.SaveChangesAsync();
            return await GetTripByIdAsync(userId, tripId);
        }

        public async Task<TripResponse> GetTripByIdAsync(Guid userId, Guid tripId)
        {
            var trip = await _db.Trips
                .Include(t => t.Budget)
                .Include(t => t.TripDays)
                    .ThenInclude(td => td.TripLocations)
                        .ThenInclude(tl => tl.Location)
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            return MapToResponse(trip);
        }

        public async Task<PagedResult<TripResponse>> GetUserTripsAsync(Guid userId, TripSearchRequest req)
        {
            var query = _db.Trips
                .Include(t => t.Budget)
                .Include(t => t.TripDays).ThenInclude(td => td.TripLocations).ThenInclude(tl => tl.Location)
                .Where(t => t.UserId == userId);

            if (req.Status.HasValue)
                query = query.Where(t => t.Status == req.Status.Value);

            var total = await query.CountAsync();

            var trips = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return new PagedResult<TripResponse>
            {
                Items = trips.Select(MapToResponse).ToList(),
                TotalCount = total,
                Page = req.Page,
                PageSize = req.PageSize
            };
        }

        public async Task DeleteTripAsync(Guid userId, Guid tripId)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");
            _db.Trips.Remove(trip);
            await _db.SaveChangesAsync();
        }

        public async Task<TripResponse> UpdateStatusAsync(Guid userId, Guid tripId, TripStatus newStatus)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            ValidateStatusTransition(trip.Status, newStatus);

            trip.Status = newStatus;
            await _db.SaveChangesAsync();
            return await GetTripByIdAsync(userId, tripId);
        }

        private static void ValidateStatusTransition(TripStatus current, TripStatus next)
        {
            bool valid = (current, next) switch
            {
                (TripStatus.Draft,      TripStatus.Planning)   => true,
                (TripStatus.Draft,      TripStatus.Cancelled)  => true,
                (TripStatus.Planning,   TripStatus.Active)     => true,
                (TripStatus.Planning,   TripStatus.Cancelled)  => true,
                (TripStatus.Active,     TripStatus.Completed)  => true,
                (TripStatus.Active,     TripStatus.Cancelled)  => true,
                _ => false
            };

            if (!valid)
                throw new InvalidOperationException($"Cannot transition trip from {current} to {next}.");
        }

        private static string GenerateShareToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
                   .Replace("+", "-").Replace("/", "_").Replace("=", "")[..32];

        private static TripResponse MapToResponse(Trip t) => new()
        {
            TripId                 = t.TripId,
            DestinationGovernorate = t.DestinationGovernorate,
            StartDate              = t.StartDate,
            EndDate                = t.EndDate,
            DurationDays           = t.DurationDays,
            TotalBudgetEgp         = t.TotalBudgetEgp,
            Status                 = t.Status.ToString(),
            ShareToken             = t.ShareToken,
            IsPublic               = t.IsPublic,
            Budget = t.Budget == null ? null : new BudgetSummary
            {
                Total         = t.Budget.TotalBudgetEgp,
                Accommodation = t.Budget.AccommodationAllocationEgp,
                Food          = t.Budget.FoodAllocationEgp,
                Transport     = t.Budget.TransportAllocationEgp,
                Activities    = t.Budget.ActivitiesAllocationEgp,
                ActualSpent   = t.Budget.ActualSpentEgp,
                VariancePct   = t.Budget.BudgetVariancePct
            },
            Days = t.TripDays
                .OrderBy(d => d.DayNumber)
                .Select(d => new TripDayResponse
                {
                    DayNumber = d.DayNumber,
                    Date      = d.Date,
                    Locations = d.TripLocations
                        .OrderBy(tl => tl.SequenceOrder)
                        .Select(tl => new TripLocationResponse
                        {
                            LocationId      = tl.Location.LocationId,
                            NameEn          = tl.Location.NameEn,
                            NameAr          = tl.Location.NameAr,
                            Category        = tl.Location.Category.ToString(),
                            TimeSlot        = tl.TimeSlot,
                            DurationMinutes = tl.VisitDurationMinutes,
                            Latitude        = tl.Location.Latitude,
                            Longitude       = tl.Location.Longitude,
                            IsHiddenGem     = tl.Location.IsHiddenGem
                        }).ToList()
                }).ToList()
        };
    }
}
