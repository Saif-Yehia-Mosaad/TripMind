using System.Threading.Tasks;
using TripMind.Application.Services;

namespace TripMind.Infrastructure.Email
{
    public sealed class StubEmailSender : IEmailSender
    {
        public Task SendPasswordResetOtpAsync(string toEmail, string displayName, string otp)
        {
            System.Console.WriteLine($"[STUB EMAIL] Password Reset OTP for {toEmail}: {otp}");
            return Task.CompletedTask;
        }

        public Task SendEmailVerificationOtpAsync(string toEmail, string displayName, string otp)
        {
            System.Console.WriteLine($"[STUB EMAIL] Email Verification OTP for {toEmail}: {otp}");
            return Task.CompletedTask;
        }

        public Task SendTwoFactorOtpAsync(string toEmail, string displayName, string otp)
        {
            System.Console.WriteLine($"[STUB EMAIL] 2FA OTP for {toEmail}: {otp}");
            return Task.CompletedTask;
        }
    }
}