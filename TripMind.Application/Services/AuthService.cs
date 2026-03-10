using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Auth;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;

namespace TripMind.Application.Services
{
    public sealed class AuthService
    {
        private readonly IAppDbContext _db;
        private readonly IJwtProvider _jwt;
        private readonly IPasswordHasher _hasher;
        private readonly IEmailSender _email;

        public AuthService(IAppDbContext db, IJwtProvider jwt, IPasswordHasher hasher, IEmailSender email)
        {
            _db = db; _jwt = jwt; _hasher = hasher; _email = email;
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest req, string? ip = null)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLowerInvariant().Trim()))
                throw new AuthException("An account with this email already exists.");

            var now = DateTime.UtcNow;
            var user = new User
            {
                UserId             = Guid.NewGuid(),
                Email              = req.Email.ToLowerInvariant().Trim(),
                DisplayName        = req.DisplayName.Trim(),
                PasswordHash       = _hasher.Hash(req.Password),
                RememberMe         = req.RememberMe,
                LanguagePreference = "AR",
                CreatedAt          = now,
                UpdatedAt          = now
            };

            _db.Users.Add(user);
            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.REGISTER", ip, true));
            await _db.SaveChangesAsync();

            return await IssueTokenPairAsync(user, ip);
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim());
            bool ok = user != null && _hasher.Verify(req.Password, user.PasswordHash);

            _db.AuditLogs.Add(Audit(user?.UserId, "AUTH.LOGIN", ip, ok,
                ok ? null : $"Failed attempt for: {req.Email}"));
            await _db.SaveChangesAsync();

            if (!ok) throw new AuthException("Invalid email or password.");

            if (user!.RememberMe != req.RememberMe)
            {
                user.RememberMe = req.RememberMe;
                user.UpdatedAt  = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return await IssueTokenPairAsync(user, ip);
        }

        public async Task<TokenResponse> RefreshAsync(string refreshToken, string? ip = null)
        {
            var stored = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored == null || !stored.IsActive)
                throw new AuthException("Invalid or expired refresh token.");

            var newRaw = _jwt.GenerateRefreshToken();
            stored.IsRevoked       = true;
            stored.ReplacedByToken = newRaw;

            _db.RefreshTokens.Add(new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId         = stored.UserId,
                Token          = newRaw,
                ExpiresAt      = DateTime.UtcNow.AddDays(_jwt.RefreshTokenLifetimeDays),
                CreatedByIp    = ip,
                CreatedAt      = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            var (access, expiresIn) = _jwt.GenerateAccessToken(stored.User);
            return ToResponse(stored.User, access, expiresIn, newRaw);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (stored == null || !stored.IsActive) throw new AuthException("Token not found or already revoked.");
            stored.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        public async Task SendPasswordResetOtpAsync(ForgotPasswordRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim());
            if (user == null) { await Task.Delay(200); return; }

            string otp = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
            user.PasswordResetToken = _hasher.Hash(otp);
            user.ResetTokenExpiry   = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt          = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.FORGOT_PASSWORD", ip, true));
            await _db.SaveChangesAsync();
            await _email.SendPasswordResetOtpAsync(user.Email, user.DisplayName, otp);
        }

        public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim())
                ?? throw new AuthException("Invalid or expired OTP.");

            if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
                throw new AuthException("OTP has expired. Please request a new one.");

            if (user.PasswordResetToken == null || !_hasher.Verify(req.Otp, user.PasswordResetToken))
                throw new AuthException("Invalid OTP.");

            string resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = _hasher.Hash(resetToken);
            user.ResetTokenExpiry   = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt          = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.OTP_VERIFIED", ip, true));
            await _db.SaveChangesAsync();

            return new VerifyOtpResponse { ResetToken = resetToken };
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim())
                ?? throw new AuthException("Invalid request.");

            if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
                throw new AuthException("Reset token has expired.");

            if (user.PasswordResetToken == null || !_hasher.Verify(req.ResetToken, user.PasswordResetToken))
                throw new AuthException("Invalid or already-used reset token.");

            if (_hasher.Verify(req.NewPassword, user.PasswordHash))
                throw new AuthException("New password must be different from the current password.");

            user.PasswordHash       = _hasher.Hash(req.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry   = null;
            user.UpdatedAt          = DateTime.UtcNow;

            var active = await _db.RefreshTokens
                .Where(r => r.UserId == user.UserId && !r.IsRevoked)
                .ToListAsync();
            foreach (var t in active) t.IsRevoked = true;

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.PASSWORD_RESET", ip, true));
            await _db.SaveChangesAsync();

            return new ResetPasswordResponse();
        }

        private async Task<TokenResponse> IssueTokenPairAsync(User user, string? ip)
        {
            var stale = await _db.RefreshTokens
                .Where(r => r.UserId == user.UserId && !r.IsRevoked && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            foreach (var t in stale) t.IsRevoked = true;

            var rawRefresh = _jwt.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId         = user.UserId,
                Token          = rawRefresh,
                ExpiresAt      = DateTime.UtcNow.AddDays(_jwt.RefreshTokenLifetimeDays),
                CreatedByIp    = ip,
                CreatedAt      = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            var (access, expiresIn) = _jwt.GenerateAccessToken(user);
            return ToResponse(user, access, expiresIn, rawRefresh);
        }

        private static TokenResponse ToResponse(User u, string access, int expiresIn, string refresh) => new()
        {
            AccessToken        = access,
            ExpiresIn          = expiresIn,
            RefreshToken       = refresh,
            UserId             = u.UserId,
            DisplayName        = u.DisplayName,
            Email              = u.Email,
            ProfilePhotoUrl    = u.ProfilePhotoUrl,
            LanguagePreference = u.LanguagePreference
        };

        private static AuditLog Audit(Guid? uid, string type, string? ip, bool ok, string? details = null) => new()
        {
            AuditLogId = Guid.NewGuid(),
            UserId     = uid,
            EventType  = type,
            IpAddress  = ip,
            Success    = ok,
            Details    = details,
            CreatedAt  = DateTime.UtcNow
        };
    }

    public interface IPasswordHasher
    {
        string Hash(string plaintext);
        bool Verify(string plaintext, string hash);
    }

    public interface IEmailSender
    {
        Task SendPasswordResetOtpAsync(string toEmail, string displayName, string otp);
    }

    public sealed class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
    }
}
