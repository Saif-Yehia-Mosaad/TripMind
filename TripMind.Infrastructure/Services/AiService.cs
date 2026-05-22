using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripMind.Application.DTOs.Ai;

namespace TripMind.Infrastructure.Services
{
    public sealed class AiService
    {
        private readonly HttpClient _http;
        private readonly string _webhookUrl;
        private readonly string _recommendUrl;
        private readonly ILogger<AiService> _logger;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public AiService(HttpClient http, IConfiguration config, ILogger<AiService> logger)
        {
            _http = http;
            _logger = logger;
            _webhookUrl = config["Ai:WebhookUrl"] ?? throw new InvalidOperationException("Ai:WebhookUrl is not configured.");
            _recommendUrl = config["Ai:RecommendUrl"] ?? throw new InvalidOperationException("Ai:RecommendUrl is not configured.");

            _http.Timeout = TimeSpan.FromSeconds(120);
        }

        public async Task<object> GeneratePlanAsync(AiSearchRequest req)
        {
            var payload = new
            {
                city = req.City,
                days = req.Days,
                budget = req.Budget,
                interests = req.Interests
            };

            _logger.LogInformation("Sending AI request for city: {City}, days: {Days}", req.City, req.Days);
            return await PostAsync(_webhookUrl, payload);
        }

        public async Task<object> GetRecommendationsAsync(RecommendRequest req)
        {
            var payload = new
            {
                user_interests = req.UserInterests,
                top_n = req.TopN,
                pool_size = req.PoolSize,
                city_filter = req.CityFilter
            };

            _logger.LogInformation("Sending recommendation request, interests: {Interests}",
                string.Join(", ", req.UserInterests));
            return await PostAsync(_recommendUrl, payload);
        }

        private async Task<object> PostAsync(string url, object payload)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(url, content);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Request timed out: {Url}", url);
                throw new InvalidOperationException("Service timed out. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed: {Url}", url);
                throw new InvalidOperationException("Service is unavailable. Please try again later.");
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Service error {Status}: {Body}", response.StatusCode, json);
                throw new InvalidOperationException("Service returned an error. Please try again.");
            }

            return JsonSerializer.Deserialize<object>(json, JsonOpts)
                ?? throw new InvalidOperationException("Service returned empty response.");
        }
    }
}