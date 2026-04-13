using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TripMind.Application.Services;

namespace TripMind.Infrastructure.Email
{
    public sealed class BrevoEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private const string FromEmail = "saifyehia58@gmail.com";
        private const string FromName = "TripMind";
        private const string ApiUrl = "https://api.brevo.com/v3/smtp/email";

        public BrevoEmailSender(string apiKey) => _apiKey = apiKey;

        public Task SendPasswordResetOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(toEmail, displayName,
                subject: "TripMind — Password Reset Code",
                html: $"<p>Hi {displayName},</p><p>Your password reset code is: <strong>{otp}</strong></p><p>Valid for 15 minutes.</p>");

        public Task SendEmailVerificationOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(toEmail, displayName,
                subject: "TripMind — Verify Your Email",
                html: $"<p>Hi {displayName},</p><p>Your verification code is: <strong>{otp}</strong></p><p>Valid for 15 minutes.</p>");

        public Task SendTwoFactorOtpAsync(string toEmail, string displayName, string otp) =>
            SendAsync(toEmail, displayName,
                subject: "TripMind — Two-Factor Authentication Code",
                html: $"<p>Hi {displayName},</p><p>Your 2FA code is: <strong>{otp}</strong></p><p>Valid for 10 minutes.</p>");

        private async Task SendAsync(string toEmail, string displayName, string subject, string html)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("api-key", _apiKey);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new { name = FromName, email = FromEmail },
                to = new[] { new { email = toEmail, name = displayName } },
                subject = subject,
                htmlContent = html
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await http.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo error {response.StatusCode}: {error}");
            }
        }
    }
}