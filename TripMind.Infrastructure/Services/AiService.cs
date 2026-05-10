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
        private readonly ILogger<AiService> _logger;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public AiService(HttpClient http, IConfiguration config, ILogger<AiService> logger)
        {
            _http = http;
            _logger = logger;
            _webhookUrl = config["Ai:WebhookUrl"]
                ?? throw new InvalidOperationException("Ai:WebhookUrl is not configured.");

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

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogInformation("Sending AI request for city: {City}, days: {Days}", req.City, req.Days);

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(_webhookUrl, content);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("AI request timed out for city: {City}", req.City);
                throw new InvalidOperationException("AI service timed out. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI request failed for city: {City}", req.City);
                throw new InvalidOperationException("AI service is unavailable. Please try again later.");
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("AI service returned error {Status}: {Body}", response.StatusCode, json);
                throw new InvalidOperationException("AI service returned an error. Please try again.");
            }

            var result = JsonSerializer.Deserialize<object>(json, JsonOpts);
            return result ?? throw new InvalidOperationException("AI service returned empty response.");
        }
    }
}