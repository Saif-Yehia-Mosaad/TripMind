using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TripMind.Application.DTOs.Ai;

namespace TripMind.Infrastructure.Services
{
    public sealed class AiService
    {
        private readonly HttpClient _http;
        private readonly string _webhookUrl;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public AiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _webhookUrl = config["Ai:WebhookUrl"] ?? throw new InvalidOperationException("Ai:WebhookUrl is not configured.");
        }

        public async Task<AiSearchResponse> SearchAsync(AiSearchRequest req)
        {
            var body = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(_webhookUrl, body);

            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException("AI service returned an error.");

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AiSearchResponse>(json, JsonOpts)
                ?? throw new InvalidOperationException("Could not parse AI response.");
        }
    }
}