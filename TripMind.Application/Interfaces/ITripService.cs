using TripMind.Application.DTOs.Trip;
using TripMind.Domain.Enums;

namespace TripMind.Application.Interfaces;

public interface ITripService
{
    Task<TripResponse> CreateTripAsync(Guid userId, CreateTripRequest req);
    Task<PagedResult<TripResponse>> GetUserTripsAsync(Guid userId, TripSearchRequest req);
    Task<TripResponse> GetTripByIdAsync(Guid userId, Guid tripId);
    Task<TripResponse> UpdateTripAsync(Guid userId, Guid tripId, UpdateTripRequest req);
    Task<TripResponse> UpdatePlanAsync(Guid userId, Guid tripId, UpdateTripRequest req);
    Task<TripResponse> RenameAsync(Guid userId, Guid tripId, RenameTripRequest req);
    Task<TripResponse> UpdateStatusAsync(Guid userId, Guid tripId, TripStatus status);
    Task DeleteTripAsync(Guid userId, Guid tripId);
    Task<string> CreateShareLinkAsync(Guid userId, Guid tripId);
    Task<PublicTripResponse> GetByShareTokenAsync(string token);

    Task<TripReviewResponse> AddTripReviewAsync(Guid userId, Guid tripId, TripReviewRequest req);
    Task<TripReviewResponse> UpdateTripReviewAsync(Guid userId, Guid tripId, TripReviewRequest req);
    Task DeleteTripReviewAsync(Guid userId, Guid tripId);
    Task<TripReviewResponse?> GetMyTripReviewAsync(Guid userId, Guid tripId);
    Task<List<TripReviewWithUserResponse>> GetTripReviewsAsync(Guid userId, Guid tripId);
    Task<List<MyTripReviewResponse>> GetMyReviewsAsync(Guid userId);
}
