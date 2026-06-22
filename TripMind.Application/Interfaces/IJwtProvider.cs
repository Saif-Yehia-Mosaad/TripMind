using System.Security.Claims;
using TripMind.Domain.Entities;

namespace TripMind.Application.Interfaces
{
    public interface IJwtProvider
    {
        (string Token, int ExpiresInSeconds) GenerateAccessToken(User user);
        string GenerateRefreshToken();
        int GetRefreshTokenLifetimeDays(bool rememberMe);
    }
}