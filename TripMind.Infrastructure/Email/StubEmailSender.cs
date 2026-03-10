using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TripMind.Application.Services;

namespace TripMind.Infrastructure.Email
{
    public sealed class StubEmailSender : IEmailSender
    {
        private readonly ILogger<StubEmailSender> _logger;
        public StubEmailSender(ILogger<StubEmailSender> logger) => _logger = logger;

        public Task SendPasswordResetOtpAsync(string toEmail, string displayName, string otp)
        {
            _logger.LogInformation("[EMAIL-STUB] OTP {Otp} -> {Email}", otp, toEmail);
            return Task.CompletedTask;
        }
    }
}
