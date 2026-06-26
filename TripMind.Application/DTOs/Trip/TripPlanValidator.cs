using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace TripMind.Application.DTOs.Trip
{
    public static class TripPlanValidator
    {
        private const int MaxRawLength = 200_000; // 200 KB

        public static void Validate(JsonElement plan)
        {
            var raw = plan.GetRawText();

            if (raw.Length > MaxRawLength)
                throw new ValidationException(
                    $"Plan payload is too large ({raw.Length} chars, max {MaxRawLength}).");

            if (plan.ValueKind != JsonValueKind.Object)
                throw new ValidationException("Plan must be a JSON object.");

            var hasDay = plan.EnumerateObject()
                .Any(x => x.Name.StartsWith("day", StringComparison.OrdinalIgnoreCase));

            if (!hasDay)
                throw new ValidationException("Plan must contain at least one day.");

            ValidateNode(plan);
        }

        private static void ValidateNode(JsonElement node)
        {
            if (node.ValueKind == JsonValueKind.Object)
            {
                bool isPlace =
                    node.TryGetProperty("name", out _) ||
                    node.TryGetProperty("photo_url", out _);

                if (isPlace)
                {
                    if (!node.TryGetProperty("name", out var name) ||
                        string.IsNullOrWhiteSpace(name.GetString()))
                    {
                        throw new ValidationException("Place name is required.");
                    }
                }

                foreach (var prop in node.EnumerateObject())
                    ValidateNode(prop.Value);
            }
            else if (node.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in node.EnumerateArray())
                    ValidateNode(item);
            }
        }
    }
}
