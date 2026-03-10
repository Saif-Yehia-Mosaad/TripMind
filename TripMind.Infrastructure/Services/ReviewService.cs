using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Review;
using TripMind.Domain.Entities;
using TripMind.Domain.Enums;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class ReviewService
    {
        private readonly TripMindDbContext _db;

        public ReviewService(TripMindDbContext db) => _db = db;

        public async Task<ReviewResponse> AddReviewAsync(Guid userId, AddReviewRequest req)
        {
            if (await _db.Reviews.AnyAsync(r => r.UserId == userId && r.LocationId == req.LocationId))
                throw new InvalidOperationException("You have already reviewed this location.");

            if (!await _db.Locations.AnyAsync(l => l.LocationId == req.LocationId))
                throw new KeyNotFoundException("Location not found.");

            var review = new Review
            {
                ReviewId         = Guid.NewGuid(),
                UserId           = userId,
                LocationId       = req.LocationId,
                Rating           = req.Rating,
                ReviewText       = req.ReviewText,
                PhotoUrl         = req.PhotoUrl,
                ModerationStatus = ModerationStatus.Pending,
                VisitedAt        = req.VisitedAt,
                CreatedAt        = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            await RecomputeAvgRatingAsync(req.LocationId);

            var user = await _db.Users.FindAsync(userId);
            return MapToResponse(review, user!.DisplayName);
        }

        public async Task<List<ReviewResponse>> GetLocationReviewsAsync(Guid locationId)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.LocationId == locationId && r.ModerationStatus == ModerationStatus.Approved)
                .OrderByDescending(r => r.HelpfulCount)
                .Select(r => new ReviewResponse
                {
                    ReviewId        = r.ReviewId,
                    UserDisplayName = r.User.DisplayName,
                    Rating          = r.Rating,
                    ReviewText      = r.ReviewText,
                    PhotoUrl        = r.PhotoUrl,
                    HelpfulCount    = r.HelpfulCount,
                    VisitedAt       = r.VisitedAt,
                    CreatedAt       = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task VoteAsync(Guid userId, Guid reviewId, bool isHelpful)
        {
            if (!await _db.Reviews.AnyAsync(r => r.ReviewId == reviewId))
                throw new KeyNotFoundException("Review not found.");

            var existing = await _db.ReviewVotes.FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

            if (existing != null)
            {
                if (existing.IsHelpful == isHelpful) return;
                existing.IsHelpful = isHelpful;
            }
            else
            {
                _db.ReviewVotes.Add(new ReviewVote
                {
                    ReviewVoteId = Guid.NewGuid(),
                    ReviewId     = reviewId,
                    UserId       = userId,
                    IsHelpful    = isHelpful,
                    VotedAt      = DateTime.UtcNow
                });
            }

            var review = await _db.Reviews.FindAsync(reviewId);
            if (review != null)
                review.HelpfulCount = await _db.ReviewVotes.CountAsync(v => v.ReviewId == reviewId && v.IsHelpful);

            await _db.SaveChangesAsync();
        }

        public async Task ReportReviewAsync(Guid reviewId)
        {
            var review = await _db.Reviews.FindAsync(reviewId)
                ?? throw new KeyNotFoundException("Review not found.");
            review.Reported          = true;
            review.ModerationStatus  = ModerationStatus.Pending;
            await _db.SaveChangesAsync();
        }

        private async Task RecomputeAvgRatingAsync(Guid locationId)
        {
            var location = await _db.Locations.FindAsync(locationId);
            if (location == null) return;

            var avg = await _db.Reviews
                .Where(r => r.LocationId == locationId && r.ModerationStatus == ModerationStatus.Approved)
                .Select(r => (double)r.Rating)
                .ToListAsync();

            location.AvgRating = avg.Count == 0 ? 0f : (float)avg.Average();
        }

        private static ReviewResponse MapToResponse(Review r, string displayName) => new()
        {
            ReviewId        = r.ReviewId,
            UserDisplayName = displayName,
            Rating          = r.Rating,
            ReviewText      = r.ReviewText,
            PhotoUrl        = r.PhotoUrl,
            HelpfulCount    = r.HelpfulCount,
            VisitedAt       = r.VisitedAt,
            CreatedAt       = r.CreatedAt
        };
    }
}
