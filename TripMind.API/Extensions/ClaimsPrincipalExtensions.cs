using System;
using System.Security.Claims;

namespace TripMind.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identity.");

            return userId;
        }
    }
}