using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TripMind.Domain.Entities;
using TripMind.Infrastructure.Persistence;

namespace TripMind.API.Middleware
{
    public sealed class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next; _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            bool isAuth = ctx.Request.Path.StartsWithSegments("/api/v1/auth", StringComparison.OrdinalIgnoreCase);
            await _next(ctx);
            if (!isAuth) return;

            try { await WriteAsync(ctx); }
            catch (Exception ex) { _logger.LogError(ex, "AuditLogMiddleware failed."); }
        }

        private static async Task WriteAsync(HttpContext ctx)
        {
            var db = ctx.RequestServices.GetRequiredService<TripMindDbContext>();

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
                EventType  = DeriveEvent(path, ctx.Response.StatusCode),
                IpAddress  = ip,
                UserAgent  = ua?.Length > 512 ? ua[..512] : ua,
                Success    = ok,
                Details    = ok ? null : $"HTTP {ctx.Response.StatusCode}",
                CreatedAt  = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        private static string DeriveEvent(string path, int code)
        {
            string s = code is >= 200 and < 300 ? "SUCCESS" : "FAILURE";
            if (path.EndsWith("/register",         StringComparison.OrdinalIgnoreCase)) return $"AUTH.REGISTER.{s}";
            if (path.EndsWith("/login",            StringComparison.OrdinalIgnoreCase)) return $"AUTH.LOGIN.{s}";
            if (path.EndsWith("/refresh",          StringComparison.OrdinalIgnoreCase)) return $"AUTH.REFRESH.{s}";
            if (path.EndsWith("/revoke",           StringComparison.OrdinalIgnoreCase)) return $"AUTH.REVOKE.{s}";
            if (path.EndsWith("/forgot-password",  StringComparison.OrdinalIgnoreCase)) return $"AUTH.FORGOT.{s}";
            if (path.EndsWith("/verify-otp",       StringComparison.OrdinalIgnoreCase)) return $"AUTH.OTP.{s}";
            if (path.EndsWith("/reset-password",   StringComparison.OrdinalIgnoreCase)) return $"AUTH.RESET.{s}";
            return $"AUTH.UNKNOWN.{s}";
        }
    }
}
