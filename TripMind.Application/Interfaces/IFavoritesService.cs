using TripMind.Application.DTOs.Favorite;
using TripMind.Application.DTOs.Trip;

namespace TripMind.Application.Interfaces;

public interface IFavoritesService
{
    Task<FavoritePlaceResponse> AddFavoritePlaceAsync(Guid userId, FavoritePlaceRequest req);
    Task RemoveFavoritePlaceAsync(Guid userId, string placeId);
    Task<PagedResult<FavoritePlaceResponse>> GetFavoritePlacesAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20);
    Task<FavoriteTripResponse> AddFavoriteTripAsync(Guid userId, Guid tripId);
    Task RemoveFavoriteTripAsync(Guid userId, Guid tripId);
    Task<List<FavoriteTripResponse>> GetFavoriteTripsAsync(Guid userId);
}
