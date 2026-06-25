using TripMind.Application.DTOs.User;

namespace TripMind.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetProfileAsync(Guid userId);

        Task<UserProfileResponse> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequest req);

        Task<UserDashboardResponse> GetDashboardAsync(Guid userId);

        Task UpdateInterestsAsync(
            Guid userId,
            List<string> interests);

        Task DeleteAccountAsync(Guid userId);
    }
}