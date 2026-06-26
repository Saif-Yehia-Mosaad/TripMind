using System.Text.Json;
using TripMind.Application.DTOs.Ai;

namespace TripMind.Application.Interfaces
{
    public interface IAiService
    {
        Task<JsonElement> GeneratePlanAsync(GeneratePlanRequest req);

        Task<JsonElement> ChatAsync(ChatRequest req);

        Task<JsonElement> EditAsync(EditRequest req);

        Task<JsonElement> HomeAsync(HomeRequest req);

        Task<JsonElement> RecommendAsync(RecommendRequest req);

        Task<JsonElement> SearchPlacesAsync(SearchPlacesRequest req);

        Task<JsonElement> NearbyAsync(NearbyRequest req);

        Task<JsonElement> TopRatedAsync(TopRatedRequest req);

        Task<JsonElement> GetPlacesAsync(GetPlacesRequest req);

        Task<JsonElement> GetPlaceByIdAsync(string placeId);
    }
}
