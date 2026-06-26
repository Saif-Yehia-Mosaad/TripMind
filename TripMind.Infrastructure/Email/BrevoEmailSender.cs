using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TripMind.Application.Services;

namespace TripMind.Infrastructure.Email
{
    public sealed class BrevoEmailSender : IEmailSender
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        private const string FromEmail = "saifyehia58@gmail.com";
        private const string FromName = "TripMind";
        private const string ApiUrl = "https://api.brevo.com/v3/smtp/email";

        public BrevoEmailSender(
            HttpClient http,
            IConfiguration configuration)
        {
            _http = http;
            _apiKey = configuration["Email:ApiKey"]
                ?? throw new InvalidOperationException("Email:ApiKey is missing.");

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task SendPasswordResetOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(
                toEmail,
                displayName,
                "TripMind � Password Reset Code",
                $"<p>Hi {displayName},</p><p>Your password reset code is: <strong>{otp}</strong></p><p>Valid for 15 minutes.</p>");

        public Task SendEmailVerificationOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(
                toEmail,
                displayName,
                "TripMind � Verify Your Email",
                $"<p>Hi {displayName},</p><p>Your verification code is: <strong>{otp}</strong></p><p>Valid for 15 minutes.</p>");

        public Task SendTwoFactorOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(
                toEmail,
                displayName,
                "TripMind � Two-Factor Authentication Code",
                $"<p>Hi {displayName},</p><p>Your 2FA code is: <strong>{otp}</strong></p><p>Valid for 10 minutes.</p>");

        private async Task SendAsync(
            string toEmail,
            string displayName,
            string subject,
            string html)
        {
            var payload = new
            {
                sender = new
                {
                    name = FromName,
                    email = FromEmail
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = displayName
                    }
                },
                subject,
                htmlContent = html
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);

            request.Headers.Add("api-key", _apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Brevo error {(int)response.StatusCode}: {error}");
            }
        }
    }
}
