using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripMind.Application.DTOs.Ai
{
    public sealed class DayFieldConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            if (reader.TokenType == JsonTokenType.String)
            {
                var val = reader.GetString() ?? "";
                if (val.Equals("accommodation", StringComparison.OrdinalIgnoreCase))
                    return 0;

                if (int.TryParse(val, out var n))
                    return n;

                throw new JsonException($"Invalid day value: '{val}'. Expected int or 'accommodation'.");
            }

            throw new JsonException($"Unexpected token type for 'day': {reader.TokenType}.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);
    }
}