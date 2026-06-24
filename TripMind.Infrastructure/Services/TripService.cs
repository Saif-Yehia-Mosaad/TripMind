using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var destination = ResolveDestination(req.DestinationGovernorate, req.City);
            var totalBudget = ResolveBudget(req.TotalBudgetEgp, req.Budget);
            var now = DateTime.UtcNow;

            ValidateDates(req.StartDate, req.EndDate);

            var trip = new Trip
            {
                TripId = Guid.NewGuid(),
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(req.Title) ? $"{destination} Trip" : req.Title.Trim(),
                DestinationGovernorate = destination,
                StartDate = req.StartDate.Date,
                EndDate = req.EndDate.Date,
                DurationDays = GetDurationDays(req.StartDate, req.EndDate),
                People = req.People ?? 0,
                TotalBudgetEgp = totalBudget,
                TotalCost = req.TotalCost ?? 0,
                SessionId = string.IsNullOrWhiteSpace(req.SessionId) ? Guid.NewGuid().ToString("N") : req.SessionId.Trim(),
                CollectedJson = req.Collected.HasValue ? req.Collected.Value.GetRawText() : null,
                PlanJson = req.Plan.HasValue ? req.Plan.Value.GetRawText() : "{}",
                Status = req.Status ?? TripStatus.Draft,
                IsPublic = req.IsPublic ?? false,
                ShareToken = GenerateShareToken(),
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync();

            return MapToResponse(trip, includePlan: true);
        }

        private static int ResolveBudget(int? totalBudgetEgp, int? budget, bool allowNull = false)
        {
            if (totalBudgetEgp.HasValue)
                return totalBudgetEgp.Value;

            if (budget.HasValue)
                return budget.Value;

            if (allowNull)
                return 0;

            throw new InvalidOperationException("Budget is required.");
        }

        public async Task<TripResponse> UpdateTripAsync(Guid userId, Guid tripId, UpdateTripRequest req)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            EnsureEditable(trip);

            ApplyUpdate(trip, req);

            if (req.StartDate.HasValue || req.EndDate.HasValue)
                ValidateDates(trip.StartDate, trip.EndDate);

            if (req.StartDate.HasValue || req.EndDate.HasValue)
                trip.DurationDays = GetDurationDays(trip.StartDate, trip.EndDate);

            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return MapToResponse(trip, includePlan: true);
        }

        public Task<TripResponse> UpdatePlanAsync(Guid userId, Guid tripId, UpdateTripRequest req)
            => UpdateTripAsync(userId, tripId, req);

        public async Task<TripResponse> RenameAsync(Guid userId, Guid tripId, RenameTripRequest req)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            EnsureEditable(trip);

            trip.Title = req.Title.Trim();
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return MapToResponse(trip, includePlan: false);
        }

        public async Task<TripResponse> GetTripByIdAsync(Guid userId, Guid tripId)
        {
            var trip = await _db.Trips
                .FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            return MapToResponse(trip, includePlan: true);
        }

        public async Task<TripResponse> GetByShareTokenAsync(string token)
        {
            var trip = await _db.Trips
                .FirstOrDefaultAsync(t => t.ShareToken == token)
                ?? throw new KeyNotFoundException("Shared trip not found.");

            return MapToResponse(trip, includePlan: true);
        }

        public async Task<PagedResult<TripResponse>> GetUserTripsAsync(Guid userId, TripSearchRequest req)
        {
            var query = _db.Trips.Where(t => t.UserId == userId);

            if (req.Status.HasValue)
                query = query.Where(t => t.Status == req.Status.Value);

            var total = await query.CountAsync();

            var trips = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return new PagedResult<TripResponse>
            {
                Items = trips.Select(t => MapToResponse(t, includePlan: false)).ToList(),
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
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return MapToResponse(trip, includePlan: false);
        }

        public Task<TripResponse> ConfirmAsync(Guid userId, Guid tripId)
            => UpdateStatusAsync(userId, tripId, TripStatus.Upcoming);

        public Task<TripResponse> CompleteAsync(Guid userId, Guid tripId)
            => UpdateStatusAsync(userId, tripId, TripStatus.Completed);

        public async Task<string> CreateShareLinkAsync(Guid userId, Guid tripId)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            trip.ShareToken ??= GenerateShareToken();
            trip.IsPublic = true;
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return trip.ShareToken;
        }

        public async Task<TripReviewResponse> AddTripReviewAsync(Guid userId, Guid tripId, TripReviewRequest req)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            if (trip.Status != TripStatus.Completed)
                throw new InvalidOperationException("You can only review a completed trip.");

            var existing = await _db.TripReviews
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId);

            if (existing != null)
                throw new InvalidOperationException("You have already reviewed this trip.");

            var review = new TripReview
            {
                TripReviewId = Guid.NewGuid(),
                TripId = tripId,
                UserId = userId,
                Rating = req.Rating,
                Comment = req.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.TripReviews.Add(review);
            await _db.SaveChangesAsync();
            return MapReview(review);
        }

        public async Task<TripReviewResponse> UpdateTripReviewAsync(Guid userId, Guid tripId, TripReviewRequest req)
        {
            var review = await _db.TripReviews
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId)
                ?? throw new KeyNotFoundException("Review not found.");

            review.Rating = req.Rating;
            review.Comment = req.Comment?.Trim();

            await _db.SaveChangesAsync();
            return MapReview(review);
        }

        public async Task DeleteTripReviewAsync(Guid userId, Guid tripId)
        {
            var review = await _db.TripReviews
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId)
                ?? throw new KeyNotFoundException("Review not found.");

            _db.TripReviews.Remove(review);
            await _db.SaveChangesAsync();
        }

        public async Task<TripReviewResponse?> GetMyTripReviewAsync(Guid userId, Guid tripId)
        {
            var review = await _db.TripReviews
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId);

            return review == null ? null : MapReview(review);
        }

        public async Task<List<TripReviewWithUserResponse>> GetTripReviewsAsync(Guid tripId)
        {
            var tripExists = await _db.Trips.AnyAsync(t => t.TripId == tripId);
            if (!tripExists)
                throw new KeyNotFoundException("Trip not found.");

            return await _db.TripReviews
                .Where(r => r.TripId == tripId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new TripReviewWithUserResponse
                {
                    TripReviewId = r.TripReviewId,
                    TripId = r.TripId,
                    UserId = r.UserId,
                    DisplayName = r.User.DisplayName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<MyTripReviewResponse>> GetMyReviewsAsync(Guid userId) =>
            await _db.TripReviews
                .Include(r => r.Trip)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new MyTripReviewResponse
                {
                    TripReviewId = r.TripReviewId,
                    TripId = r.TripId,
                    Destination = r.Trip.DestinationGovernorate,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

        private static void ApplyUpdate(Trip trip, UpdateTripRequest req)
        {
            if (!string.IsNullOrWhiteSpace(req.Title))
                trip.Title = req.Title.Trim();

            var destination = ResolveDestination(req.DestinationGovernorate, req.City);
            if (destination != null)
                trip.DestinationGovernorate = destination;

            if (req.StartDate.HasValue)
                trip.StartDate = req.StartDate.Value.Date;

            if (req.EndDate.HasValue)
                trip.EndDate = req.EndDate.Value.Date;

            if (req.People.HasValue)
                trip.People = req.People.Value;

            var budget = ResolveBudget(req.TotalBudgetEgp, req.Budget, allowNull: true);
            if (req.TotalBudgetEgp.HasValue)
                trip.TotalBudgetEgp = req.TotalBudgetEgp.Value;
            else if (req.Budget.HasValue)
                trip.TotalBudgetEgp = req.Budget.Value;

            if (req.TotalCost.HasValue)
                trip.TotalCost = req.TotalCost.Value;

            if (req.Plan.HasValue)
                trip.PlanJson = req.Plan.Value.GetRawText();

            if (req.Collected.HasValue)
                trip.CollectedJson = req.Collected.Value.GetRawText();

            if (!string.IsNullOrWhiteSpace(req.SessionId))
                trip.SessionId = req.SessionId.Trim();

            if (req.IsPublic.HasValue)
                trip.IsPublic = req.IsPublic.Value;
        }

        private static void EnsureEditable(Trip trip)
        {
            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                throw new InvalidOperationException("Cannot edit a completed or cancelled trip.");
        }

        private static void ValidateDates(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date <= startDate.Date)
                throw new InvalidOperationException("End date must be after start date.");
        }

        private static int GetDurationDays(DateTime startDate, DateTime endDate)
            => (endDate.Date - startDate.Date).Days + 1;

        private static void ValidateStatusTransition(TripStatus current, TripStatus next)
        {
            if (current == next)
                return;

            var valid = (current, next) switch
            {
                (TripStatus.Draft, TripStatus.Upcoming) => true,
                (TripStatus.Draft, TripStatus.Cancelled) => true,
                (TripStatus.Upcoming, TripStatus.Completed) => true,
                (TripStatus.Upcoming, TripStatus.Cancelled) => true,
                _ => false
            };

            if (!valid)
                throw new InvalidOperationException($"Cannot transition trip from {current} to {next}.");
        }

        private static string GenerateShareToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")[..32];

        private static TripReviewResponse MapReview(TripReview r) => new()
        {
            TripReviewId = r.TripReviewId,
            TripId = r.TripId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        };

        private static TripResponse MapToResponse(Trip t, bool includePlan)
        {
            var (placesCount, coverImage) = AnalyzePlan(t.PlanJson);

            var duration = t.DurationDays > 0
                ? t.DurationDays
                : GetDurationDays(t.StartDate, t.EndDate);

            int? progress = t.Status == TripStatus.Draft
                ? CalculateProgress(placesCount, duration)
                : null;

            return new TripResponse
            {
                TripId = t.TripId,
                Title = t.Title,
                DestinationGovernorate = t.DestinationGovernorate,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                DurationDays = duration,
                People = t.People,
                TotalBudgetEgp = t.TotalBudgetEgp,
                TotalCost = t.TotalCost,
                Status = t.Status.ToString(),
                ShareToken = t.ShareToken,
                IsPublic = t.IsPublic,
                SessionId = t.SessionId,
                CollectedJson = includePlan ? t.CollectedJson : null,
                CoverImageUrl = coverImage,
                PlacesCount = placesCount,
                ProgressPercent = progress,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Plan = includePlan ? TryParseJson(t.PlanJson) : null
            };
        }

        private static JsonElement? TryParseJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch
            {
                return null;
            }
        }

        private static (int placesCount, string? coverImage) AnalyzePlan(string? planJson)
        {
            if (string.IsNullOrWhiteSpace(planJson))
                return (0, null);

            int count = 0;
            string? cover = null;

            try
            {
                using var doc = JsonDocument.Parse(planJson);
                Walk(doc.RootElement);
            }
            catch (JsonException)
            {
                return (0, null);
            }

            return (count, cover);

            void Walk(JsonElement el)
            {
                switch (el.ValueKind)
                {
                    case JsonValueKind.Object:
                        if (el.TryGetProperty("name", out _))
                        {
                            count++;
                            if (cover == null
                                && el.TryGetProperty("photo_url", out var photo)
                                && photo.ValueKind == JsonValueKind.String
                                && !string.IsNullOrWhiteSpace(photo.GetString()))
                            {
                                cover = photo.GetString();
                            }
                        }

                        foreach (var prop in el.EnumerateObject())
                            Walk(prop.Value);
                        break;

                    case JsonValueKind.Array:
                        foreach (var item in el.EnumerateArray())
                            Walk(item);
                        break;
                }
            }
        }

        private static int CalculateProgress(int placesCount, int days)
        {
            var expected = Math.Max(days * 6, 1);
            return Math.Min(100, (int)Math.Round(placesCount * 100.0 / expected));
        }

        private static string ResolveDestination(string? destinationGovernorate, string? city)
            => !string.IsNullOrWhiteSpace(destinationGovernorate)
                ? destinationGovernorate.Trim()
                : !string.IsNullOrWhiteSpace(city)
                    ? city.Trim()
                    : null!;
    }
}