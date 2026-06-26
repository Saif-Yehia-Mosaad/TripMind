using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TripMind.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace TripMind.API.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next; _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (Exception ex) { await HandleAsync(ctx, ex); }
        }

        private async Task HandleAsync(HttpContext ctx, Exception ex)
        {
            if (ctx.Response.HasStarted)
                return;

            var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();

            (int status, string title) = ex switch
            {
                AuthException => (401, "Authentication failed."),
                KeyNotFoundException => (404, "Resource not found."),
                InvalidOperationException => (400, "Invalid operation."),
                UnauthorizedAccessException => (403, "Access denied."),
                ValidationException => (400, "Validation failed."),
                _ => (500, "An unexpected error occurred.")
            };

            if (status == 500)
                _logger.LogError(ex, "{Method} {Path}", ctx.Request.Method, ctx.Request.Path);

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;

            var response = new
            {
                type = $"https://httpstatuses.com/{status}",
                title,
                status,
                detail = env.IsDevelopment() ? ex.ToString() : "An internal error occurred.",
                instance = ctx.Request.Path.Value,
                traceId = ctx.TraceIdentifier
            };

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ));
        }
    }
}
