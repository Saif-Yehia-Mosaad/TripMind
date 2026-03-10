using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Itinerary;
using TripMind.Domain.Entities;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class ItineraryService
    {
        private readonly TripMindDbContext _db;
        public ItineraryService(TripMindDbContext db) => _db = db;

        public async Task<List<SavedItineraryResponse>> GetSavedAsync(Guid userId) =>
            await _db.SavedItineraries
                .Include(si => si.Trip)
                .Where(si => si.UserId == userId)
                .OrderByDescending(si => si.SavedAt)
                .Select(si => new SavedItineraryResponse
                {
                    SavedItineraryId = si.SavedItineraryId,
                    CustomName       = si.CustomName,
                    IsFavorite       = si.IsFavorite,
                    SavedAt          = si.SavedAt,
                    TripId           = si.Trip.TripId,
                    Destination      = si.Trip.DestinationGovernorate,
                    StartDate        = si.Trip.StartDate,
                    EndDate          = si.Trip.EndDate,
                    DurationDays     = si.Trip.DurationDays,
                    Status           = si.Trip.Status.ToString()
                })
                .ToListAsync();

        public async Task<SavedItineraryResponse> SaveAsync(Guid userId, SaveItineraryRequest req)
        {
            if (!await _db.Trips.AnyAsync(t => t.TripId == req.TripId && t.UserId == userId))
                throw new KeyNotFoundException("Trip not found.");

            var item = new SavedItinerary
            {
                SavedItineraryId = Guid.NewGuid(),
                UserId           = userId,
                TripId           = req.TripId,
                CustomName       = req.CustomName,
                IsFavorite       = req.IsFavorite,
                SavedAt          = DateTime.UtcNow
            };

            _db.SavedItineraries.Add(item);
            await _db.SaveChangesAsync();

            return (await GetSavedAsync(userId)).First(s => s.SavedItineraryId == item.SavedItineraryId);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var item = await _db.SavedItineraries
                .FirstOrDefaultAsync(si => si.SavedItineraryId == id && si.UserId == userId)
                ?? throw new KeyNotFoundException("Saved itinerary not found.");
            _db.SavedItineraries.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task<object> GetByShareTokenAsync(string token)
        {
            var trip = await _db.Trips
                .Include(t => t.TripDays).ThenInclude(d => d.TripLocations).ThenInclude(tl => tl.Location)
                .Include(t => t.Budget)
                .FirstOrDefaultAsync(t => t.ShareToken == token && t.IsPublic)
                ?? throw new KeyNotFoundException("Itinerary not found or share link expired.");

            return new
            {
                trip.TripId,
                trip.DestinationGovernorate,
                trip.StartDate,
                trip.EndDate,
                trip.DurationDays,
                Status = trip.Status.ToString(),
                Budget = trip.Budget == null ? null : new
                {
                    trip.Budget.TotalBudgetEgp,
                    trip.Budget.AccommodationAllocationEgp,
                    trip.Budget.FoodAllocationEgp,
                    trip.Budget.TransportAllocationEgp,
                    trip.Budget.ActivitiesAllocationEgp,
                    trip.Budget.ActualSpentEgp
                },
                Days = trip.TripDays.OrderBy(d => d.DayNumber).Select(d => new
                {
                    d.DayNumber,
                    d.Date,
                    Locations = d.TripLocations.OrderBy(tl => tl.SequenceOrder).Select(tl => new
                    {
                        tl.Location.LocationId,
                        tl.Location.NameEn,
                        tl.Location.NameAr,
                        tl.TimeSlot,
                        tl.VisitDurationMinutes
                    })
                })
            };
        }
    }
}
