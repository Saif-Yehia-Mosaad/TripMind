using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TripMind.API.Middleware
{
    public sealed class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _store = new();
        private static readonly Timer _cleanup = new(_ =>
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-WindowSeconds * 2);
            foreach (var key in _store.Keys)
                if (_store.TryGetValue(key, out var v) && v.WindowStart < cutoff)
                    _store.TryRemove(key, out var _);
        }, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        private const int MaxRequests   = 100;
        private const int WindowSeconds = 60;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next; _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            string ip  = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var    now = DateTime.UtcNow;

            _store.AddOrUpdate(ip, _ => (1, now), (_, existing) =>
                (now - existing.WindowStart).TotalSeconds >= WindowSeconds
                    ? (1, now)
                    : (existing.Count + 1, existing.WindowStart));

            if (_store.TryGetValue(ip, out var entry) && entry.Count > MaxRequests)
            {
                _logger.LogWarning("Rate limit exceeded: {Ip}", ip);
                ctx.Response.StatusCode = 429;
                ctx.Response.Headers["Retry-After"] = WindowSeconds.ToString();
                await ctx.Response.WriteAsJsonAsync(new { status = 429, title = "Too Many Requests" });
                return;
            }

            await _next(ctx);
        }
    }
}
