using System;

namespace TripMind.Infrastructure.Services
{
    public sealed class AiServiceException : Exception
    {
        public int StatusCode { get; }
        public string RawBody { get; }

        public AiServiceException(int statusCode, string rawBody)
            : base($"AI service returned {statusCode}")
        {
            StatusCode = statusCode;
            RawBody = rawBody;
        }
    }
}
