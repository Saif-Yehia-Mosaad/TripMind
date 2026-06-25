using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;

namespace TripMind.API.Middleware
{
    public sealed class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        private static readonly (string Path, string Method)[] SensitivePaths =
{
    ("/api/v1/auth", "*"),

    ("/api/v1/users/me", "PATCH"),
    ("/api/v1/users/me", "DELETE"),
    ("/api/v1/users/me/photo", "POST"),

    ("/api/v1/trips", "POST"),
    ("/api/v1/trips", "DELETE"),
    ("/api/v1/trips", "PATCH"),
    ("/api/v1/trips", "PUT"),

    ("/api/v1/trips/share", "GET"),

    ("/api/v1/favorites/places", "*"),
    ("/api/v1/favorites/trips", "*")
};

        public AuditLogMiddleware(
            RequestDelegate next,
            ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? string.Empty;
            var method = ctx.Request.Method;

            bool isSensitive = SensitivePaths.Any(p =>
                path.StartsWith(p.Path, StringComparison.OrdinalIgnoreCase) &&
                (p.Method == "*" || p.Method.Equals(method, StringComparison.OrdinalIgnoreCase)));

            await _next(ctx);

            if (!isSensitive)
                return;

            try
            {
                await WriteAsync(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuditLogMiddleware failed.");
            }
        }

        private static async Task WriteAsync(HttpContext ctx)
        {
            var db = ctx.RequestServices.GetRequiredService<IAppDbContext>();
            Guid? userId = null;
            var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? ctx.User.FindFirstValue("sub");
            if (sub != null && Guid.TryParse(sub, out var parsed)) userId = parsed;

            string? ip = ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd)
                ? fwd.ToString().Split(',')[0].Trim()
                : ctx.Connection.RemoteIpAddress?.ToString();

            string? ua   = ctx.Request.Headers["User-Agent"].ToString();
            bool    ok   = ctx.Response.StatusCode is >= 200 and < 300;
            string  path = ctx.Request.Path.Value ?? string.Empty;

            db.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                UserId     = userId,
                EventType = DeriveEvent(path, ctx.Request.Method, ctx.Response.StatusCode),
                IpAddress  = ip,
                UserAgent  = ua?.Length > 512 ? ua[..512] : ua,
                Success    = ok,
                Details    = ok ? null : $"HTTP {ctx.Response.StatusCode}",
                CreatedAt  = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        private static string DeriveEvent(string path, string method, int code)
        {
            string s = code is >= 200 and < 300 ? "SUCCESS" : "FAILURE";

            path = path.ToLowerInvariant();

            // =========================
            // Authentication
            // =========================
            if (path.EndsWith("/register"))
                return $"AUTH.REGISTER.{s}";

            if (path.EndsWith("/login"))
                return $"AUTH.LOGIN.{s}";

            if (path.EndsWith("/refresh"))
                return $"AUTH.REFRESH.{s}";

            if (path.EndsWith("/revoke"))
                return $"AUTH.REVOKE.{s}";

            if (path.EndsWith("/logout"))
                return $"AUTH.LOGOUT.{s}";

            if (path.EndsWith("/email/verify"))
                return $"AUTH.EMAIL_VERIFY.{s}";

            if (path.EndsWith("/email/resend-otp"))
                return $"AUTH.EMAIL_RESEND.{s}";

            if (path.EndsWith("/login/verify"))
                return $"AUTH.LOGIN_OTP.{s}";

            if (path.EndsWith("/login/resend-otp"))
                return $"AUTH.LOGIN_OTP_RESEND.{s}";

            if (path.EndsWith("/password/forgot"))
                return $"AUTH.FORGOT_PASSWORD.{s}";

            if (path.EndsWith("/password/verifyotp"))
                return $"AUTH.PASSWORD_VERIFY_OTP.{s}";

            if (path.EndsWith("/password/reset"))
                return $"AUTH.PASSWORD_RESET.{s}";

            if (path.EndsWith("/password/change"))
                return $"AUTH.CHANGE_PASSWORD.{s}";

            if (path.EndsWith("/2fa/initiate"))
                return $"AUTH.TWO_FACTOR_INITIATE.{s}";

            if (path.EndsWith("/2fa/confirm"))
                return $"AUTH.TWO_FACTOR_CONFIRM.{s}";

            if (path.EndsWith("/2fa/disable"))
                return $"AUTH.TWO_FACTOR_DISABLE.{s}";

            if (path.EndsWith("/2fa/resend-otp"))
                return $"AUTH.TWO_FACTOR_RESEND.{s}";

            // =========================
            // User Profile
            // =========================
            if (path.EndsWith("/users/me/photo"))
                return $"PROFILE.PHOTO_UPLOAD.{s}";

            if (path.EndsWith("/users/me"))
            {
                if (method.Equals(HttpMethods.Patch, StringComparison.OrdinalIgnoreCase))
                    return $"PROFILE.UPDATE.{s}";

                if (method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
                    return $"ACCOUNT.DELETE.{s}";
            }

            // =========================
            // Trips
            // =========================

            // POST /api/v1/trips
            if (path.Equals("/api/v1/trips") &&
                method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                return $"TRIP.CREATE.{s}";

            // DELETE /api/v1/trips/{id}
            if (path.StartsWith("/api/v1/trips/") &&
                method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
                return $"TRIP.DELETE.{s}";

            // PUT /api/v1/trips/{id}/plan
            if (path.EndsWith("/plan"))
                return $"TRIP.UPDATE.{s}";

            // PATCH /api/v1/trips/{id}/rename
            if (path.EndsWith("/rename"))
                return $"TRIP.RENAME.{s}";

            // PATCH /api/v1/trips/{id}/status
            if (path.EndsWith("/status"))
                return $"TRIP.STATUS.{s}";

            // Reviews
            if (path.Contains("/review"))
            {
                if (method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                    return $"TRIP.REVIEW_ADD.{s}";

                if (method.Equals(HttpMethods.Patch, StringComparison.OrdinalIgnoreCase))
                    return $"TRIP.REVIEW_UPDATE.{s}";

                if (method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
                    return $"TRIP.REVIEW_DELETE.{s}";

                if (method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
                    return $"TRIP.REVIEW_LIST.{s}";
            }

            // Share
            if (path.EndsWith("/share") || path.Contains("/trips/share/"))
                return $"TRIP.SHARE.{s}";

            // Favorites
            if (path.StartsWith("/api/v1/favorites/places"))
            {
                if (method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.PLACE_ADD.{s}";

                if (method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.PLACE_REMOVE.{s}";

                if (method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.PLACE_LIST.{s}";
            }

            if (path.StartsWith("/api/v1/favorites/trips"))
            {
                if (method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.TRIP_ADD.{s}";

                if (method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.TRIP_REMOVE.{s}";

                if (method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
                    return $"FAVORITE.TRIP_LIST.{s}";
            }

            return $"UNKNOWN.{s}";
        }
    }
}
