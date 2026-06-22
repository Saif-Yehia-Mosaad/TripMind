using System;
using System.Collections.Generic;
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
        private readonly ILogger<AiService> _logger;
        private readonly string _plannerUrl;
        private readonly string _chatUrl;
        private readonly string _editUrl;
        private readonly string _recommendBase;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public AiService(HttpClient http, IConfiguration config, ILogger<AiService> logger)
        {
            _http = http;
            _logger = logger;
            _http.Timeout = TimeSpan.FromSeconds(120);
            _plannerUrl = config["Ai:PlannerUrl"] ?? throw new InvalidOperationException("Ai:PlannerUrl not configured.");
            _chatUrl = config["Ai:ChatUrl"] ?? throw new InvalidOperationException("Ai:ChatUrl not configured.");
            _editUrl = config["Ai:EditUrl"] ?? throw new InvalidOperationException("Ai:EditUrl not configured.");
            _recommendBase = config["Ai:RecommendBase"] ?? throw new InvalidOperationException("Ai:RecommendBase not configured.");
        }

        // ── Generate Plan ─────────────────────────────────────────────────────
        public async Task<JsonElement> GeneratePlanAsync(GeneratePlanRequest req)
        {
            var payload = new
            {
                city = req.City,
                days = req.Days,
                budget = req.Budget,
                people = req.People,
                interests = req.Interests,
                must_include = req.MustInclude
            };
            _logger.LogInformation("GeneratePlan: {City} {Days}d {People}p", req.City, req.Days, req.People);
            return await PostAsync(_plannerUrl, payload);
        }

        // ── ChatBot ───────────────────────────────────────────────────────────
        public async Task<JsonElement> ChatAsync(ChatRequest req)
        {
            var payload = new
            {
                sessionId = req.SessionId,
                message = req.Message,
                collected = new
                {
                    destination = req.Collected.Destination,
                    days = req.Collected.Days,
                    budget = req.Collected.Budget,
                    interests = req.Collected.Interests,
                    people = req.Collected.People,
                    must_include = req.Collected.MustInclude
                },
                card_answers = req.CardAnswers
            };
            _logger.LogInformation("Chat: session={SessionId}", req.SessionId);
            return await PostAsync(_chatUrl, payload);
        }

        // ── EditBot ───────────────────────────────────────────────────────────
        public async Task<JsonElement> EditAsync(EditRequest req)
        {
            var payload = new
            {
                target_change = req.TargetChange,
                destination = req.Destination,
                days = req.Days,
                budget = req.Budget,
                people = req.People,
                interests = req.Interests,
                existing_plan = req.ExistingPlan,
                places = req.Places,
                conversation = req.Conversation
            };
            _logger.LogInformation("Edit: {TargetChange}", req.TargetChange);
            return await PostAsync(_editUrl, payload);
        }

        // ── Recommendations ───────────────────────────────────────────────────
        public async Task<JsonElement> HomeAsync(HomeRequest req)
            => await PostAsync($"{_recommendBase}/places/home",
                new { city = req.City, seed = req.Seed });

        public async Task<JsonElement> RecommendAsync(RecommendRequest req)
            => await PostAsync($"{_recommendBase}/places/recommend", new
            {
                selected_categories = req.SelectedCategories,
                filters = req.Filters,
                page = req.Page,
                limit = req.Limit,
                seed = req.Seed,
                pool_size = req.PoolSize
            });

        public async Task<JsonElement> SearchPlacesAsync(SearchPlacesRequest req)
            => await PostAsync($"{_recommendBase}/places/search",
                new { query = req.Query, filters = req.Filters, page = req.Page, limit = req.Limit });

        public async Task<JsonElement> TopRatedAsync(TopRatedRequest req)
            => await PostAsync($"{_recommendBase}/places/top-rated",
                new { filters = req.Filters, page = req.Page, limit = req.Limit });

        public async Task<JsonElement> GetPlacesAsync(GetPlacesRequest req)
            => await PostAsync($"{_recommendBase}/places/getplaces", new
            {
                city = req.City,
                category = req.Category,
                interests = req.Interests,
                min_rating = req.MinRating,
                max_rating = req.MaxRating,
                min_price = req.MinPrice,
                max_price = req.MaxPrice,
                hidden_gem = req.HiddenGem,
                sort_by = req.SortBy,
                order = req.Order,
                page = req.Page,
                limit = req.Limit
            });

        public async Task<JsonElement> GetPlaceByIdAsync(string placeId)
        {
            if (string.IsNullOrWhiteSpace(placeId))
                throw new ArgumentException("placeId cannot be null or empty.", nameof(placeId));

            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync($"{_recommendBase}/places/{Uri.EscapeDataString(placeId)}");
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException("Service timed out. Please try again.");
            }

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("GetPlace Status: {Status} | ContentType: {CT} | Body: {Body}",
                response.StatusCode,
                response.Content.Headers.ContentType?.ToString(),
                Truncate(json, 500));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException("Place not found.");

            if (!response.IsSuccessStatusCode)
            {
                var truncated = Truncate(json, 2000);
                _logger.LogError("AI service error {Status} from {Url}: {Body}",
                    response.StatusCode, $"{_recommendBase}/places/{placeId}", truncated);
                throw new AiServiceException((int)response.StatusCode, truncated);
            }

            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("AI service returned an empty response body.");

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON from {Url}: {Body}",
                    $"{_recommendBase}/places/{placeId}", Truncate(json, 500));
                throw new InvalidOperationException("Service returned an unexpected response format.");
            }
        }

        // ── Shared PostAsync ──────────────────────────────────────────────────
        private async Task<JsonElement> PostAsync(string url, object payload)
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
                _logger.LogError("Timeout: {Url}", url);
                throw new InvalidOperationException("Service timed out. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed: {Url}", url);
                throw new InvalidOperationException("Service unavailable. Please try again later.");
            }

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("AI Response | URL: {Url} | Status: {Status} | ContentType: {CT} | Body: {Body}",
                url,
                response.StatusCode,
                response.Content.Headers.ContentType?.ToString(),
                Truncate(json, 500));

            if (!response.IsSuccessStatusCode)
            {
                var truncated = Truncate(json, 2000);
                _logger.LogError("Error {Status} from {Url}: {Body}", response.StatusCode, url, truncated);
                throw new AiServiceException((int)response.StatusCode, truncated);
            }

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json, JsonOpts);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON from {Url}: {Body}", url, Truncate(json, 500));
                throw new InvalidOperationException("Service returned an unexpected response format.");
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
            return value[..maxLength];
        }
    }
}