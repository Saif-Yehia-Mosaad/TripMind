using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Google.Apis.Auth;
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

        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;

        public AuthService(IAppDbContext db, IJwtProvider jwt, IPasswordHasher hasher, IEmailSender email)
        {
            _db = db; _jwt = jwt; _hasher = hasher; _email = email;
        }

        public async Task<MessageResponse> RegisterAsync(RegisterRequest req, string? ip = null)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLowerInvariant().Trim()))
                throw new AuthException("An account with this email already exists.");

            var emailDomain = req.Email.Split('@')[1].ToLower();
            var blockedDomains = new[] { "test.com", "example.com", "mailinator.com", "tempmail.com", "guerrillamail.com", "yopmail.com" };
            if (Array.Exists(blockedDomains, d => emailDomain == d))
                throw new AuthException("Please use a valid email address.");

            var now = DateTime.UtcNow;
            string otp = GenerateOtp();

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = req.Email.ToLowerInvariant().Trim(),
                DisplayName = req.DisplayName.Trim(),
                PasswordHash = _hasher.Hash(req.Password),
                RememberMe = req.RememberMe,
                IsActive = true,
                IsEmailVerified = false,
                EmailVerificationOtp = _hasher.Hash(otp),
                EmailOtpExpiry = now.AddMinutes(15),
                LanguagePreference = "AR",
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.REGISTER", ip, true));
            await _db.SaveChangesAsync();

            try
            {
                await _email.SendEmailVerificationOtpAsync(user.Email, user.DisplayName, otp);
            }
            catch
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                throw new AuthException("Failed to send verification email. Please try again.");
            }

            return new MessageResponse { Message = "Registration successful. Please check your email to verify your account." };

        }

        public async Task<object> LoginAsync(LoginRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim());

            if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                throw new AuthException($"Account is locked. Try again after {user.LockoutEnd.Value:HH:mm} UTC.");

            bool ok = user != null && _hasher.Verify(req.Password, user.PasswordHash);

            if (!ok)
            {
                if (user != null)
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= MaxFailedAttempts)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                        user.FailedLoginAttempts = 0;
                    }
                    user.UpdatedAt = DateTime.UtcNow;
                }
                _db.AuditLogs.Add(Audit(user?.UserId, "AUTH.LOGIN", ip, false, $"Failed attempt for: {req.Email}"));
                await _db.SaveChangesAsync();
                throw new AuthException("Invalid email or password.");
            }

            if (!user!.IsActive)
                throw new AuthException("Your account has been deactivated. Please contact support.");

            if (!user.IsEmailVerified)
                throw new AuthException("Please verify your email before logging in.");

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            if (user.TwoFactorEnabled)
            {
                string otp = GenerateOtp();
                user.TwoFactorOtp = _hasher.Hash(otp);
                user.TwoFactorOtpExpiry = DateTime.UtcNow.AddMinutes(10);
                user.UpdatedAt = DateTime.UtcNow;
                _db.AuditLogs.Add(Audit(user.UserId, "AUTH.LOGIN_2FA_SENT", ip, true));
                await _db.SaveChangesAsync();
                await _email.SendTwoFactorOtpAsync(user.Email, user.DisplayName, otp);
                return new PendingTwoFactorResponse { Email = user.Email };
            }

            if (user.RememberMe != req.RememberMe)
            {
                user.RememberMe = req.RememberMe;
                user.UpdatedAt = DateTime.UtcNow;
            }

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.LOGIN", ip, true));
            await _db.SaveChangesAsync();

            return await IssueTokenPairAsync(user, ip);
        }

        public async Task<TokenResponse> VerifyLoginOtpAsync(LoginOtpRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim())
                ?? throw new AuthException("Invalid request.");

            if (user.TwoFactorOtpExpiry == null || user.TwoFactorOtpExpiry < DateTime.UtcNow)
                throw new AuthException("OTP has expired. Please login again.");

            if (user.TwoFactorOtp == null || !_hasher.Verify(req.Otp, user.TwoFactorOtp))
                throw new AuthException("Invalid OTP.");

            user.TwoFactorOtp = null;
            user.TwoFactorOtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.LOGIN_2FA_VERIFIED", ip, true));
            await _db.SaveChangesAsync();

            return await IssueTokenPairAsync(user, ip);
        }

        public async Task<MessageResponse> VerifyEmailAsync(VerifyEmailOtpRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim())
                ?? throw new AuthException("Invalid request.");

            if (user.IsEmailVerified)
                return new MessageResponse { Message = "Email is already verified." };

            if (user.EmailOtpExpiry == null || user.EmailOtpExpiry < DateTime.UtcNow)
                throw new AuthException("OTP has expired. Please request a new one.");

            if (user.EmailVerificationOtp == null || !_hasher.Verify(req.Otp, user.EmailVerificationOtp))
                throw new AuthException("Invalid OTP.");

            user.IsEmailVerified = true;
            user.EmailVerificationOtp = null;
            user.EmailOtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.EMAIL_VERIFIED", ip, true));
            await _db.SaveChangesAsync();

            return new MessageResponse { Message = "Email verified successfully. You can now login." };
        }

        public async Task<MessageResponse> ResendEmailOtpAsync(ResendEmailOtpRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim());
            if (user == null || user.IsEmailVerified) { await Task.Delay(200); return new MessageResponse { Message = "If that email is registered and unverified, a new OTP has been sent." }; }

            string otp = GenerateOtp();
            user.EmailVerificationOtp = _hasher.Hash(otp);
            user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _email.SendEmailVerificationOtpAsync(user.Email, user.DisplayName, otp);

            return new MessageResponse { Message = "If that email is registered and unverified, a new OTP has been sent." };
        }

        public async Task<MessageResponse> InitiateTwoFactorAsync(Guid userId, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new AuthException("User not found.");

            if (user.TwoFactorEnabled)
                throw new AuthException("2FA is already enabled.");

            string otp = GenerateOtp();
            user.TwoFactorOtp = _hasher.Hash(otp);
            user.TwoFactorOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _email.SendTwoFactorOtpAsync(user.Email, user.DisplayName, otp);

            return new MessageResponse { Message = "OTP sent to your email. Please confirm to enable 2FA." };
        }

        public async Task<MessageResponse> ConfirmTwoFactorAsync(Guid userId, TwoFactorConfirmRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new AuthException("User not found.");

            if (user.TwoFactorOtpExpiry == null || user.TwoFactorOtpExpiry < DateTime.UtcNow)
                throw new AuthException("OTP has expired. Please initiate 2FA again.");

            if (user.TwoFactorOtp == null || !_hasher.Verify(req.Otp, user.TwoFactorOtp))
                throw new AuthException("Invalid OTP.");

            user.TwoFactorEnabled = true;
            user.TwoFactorOtp = null;
            user.TwoFactorOtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(userId, "AUTH.2FA_ENABLED", ip, true));
            await _db.SaveChangesAsync();

            return new MessageResponse { Message = "Two-factor authentication has been enabled." };
        }

        public async Task<MessageResponse> DisableTwoFactorAsync(Guid userId, TwoFactorDisableRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new AuthException("User not found.");

            if (!user.TwoFactorEnabled)
                throw new AuthException("2FA is not enabled.");

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                throw new AuthException("Invalid password.");

            user.TwoFactorEnabled = false;
            user.TwoFactorOtp = null;
            user.TwoFactorOtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(userId, "AUTH.2FA_DISABLED", ip, true));
            await _db.SaveChangesAsync();

            return new MessageResponse { Message = "Two-factor authentication has been disabled." };
        }

        public async Task<MessageResponse> ResendTwoFactorOtpAsync(string email, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim());
            if (user == null) { await Task.Delay(200); return new MessageResponse { Message = "If valid, a new OTP has been sent." }; }

            string otp = GenerateOtp();
            user.TwoFactorOtp = _hasher.Hash(otp);
            user.TwoFactorOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _email.SendTwoFactorOtpAsync(user.Email, user.DisplayName, otp);

            return new MessageResponse { Message = "If valid, a new OTP has been sent." };
        }

        public async Task<MessageResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new AuthException("User not found.");

            if (!_hasher.Verify(req.CurrentPassword, user.PasswordHash))
                throw new AuthException("Current password is incorrect.");

            if (_hasher.Verify(req.NewPassword, user.PasswordHash))
                throw new AuthException("New password must be different from the current password.");

            user.PasswordHash = _hasher.Hash(req.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(Audit(userId, "AUTH.PASSWORD_CHANGED", ip, true));
            await _db.SaveChangesAsync();

            return new MessageResponse { Message = "Password changed successfully." };
        }

        public async Task<TokenResponse> GoogleLoginAsync(string idToken, string? ip = null)
        {
            GoogleJsonWebSignature.Payload payload;
            try { payload = await GoogleJsonWebSignature.ValidateAsync(idToken); }
            catch { throw new AuthException("Invalid Google token."); }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject)
                    ?? await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email.ToLowerInvariant());

            if (user == null)
            {
                var now = DateTime.UtcNow;
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = payload.Email.ToLowerInvariant(),
                    DisplayName = payload.Name ?? payload.Email,
                    ProfilePhotoUrl = payload.Picture,
                    GoogleId = payload.Subject,
                    PasswordHash = _hasher.Hash(Guid.NewGuid().ToString()),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.Users.Add(user);
            }
            else
            {
                if (user.GoogleId == null) user.GoogleId = payload.Subject;
                if (user.ProfilePhotoUrl == null) user.ProfilePhotoUrl = payload.Picture;
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            if (!user.IsActive)
                throw new AuthException("Your account has been deactivated. Please contact support.");

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.GOOGLE_LOGIN", ip, true));
            await _db.SaveChangesAsync();

            return await IssueTokenPairAsync(user, ip);
        }

        public async Task<TokenResponse> FacebookLoginAsync(string accessToken, string? ip = null)
        {
            string url = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={accessToken}";
            using var http = new System.Net.Http.HttpClient();
            var res = await http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                throw new AuthException("Invalid Facebook token.");

            var json = await res.Content.ReadAsStringAsync();
            var fb = System.Text.Json.JsonSerializer.Deserialize<FacebookUserInfo>(json,
                           new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? throw new AuthException("Could not parse Facebook response.");

            if (string.IsNullOrEmpty(fb.Email))
                throw new AuthException("Facebook account has no email. Please use email registration.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.FacebookId == fb.Id)
                    ?? await _db.Users.FirstOrDefaultAsync(u => u.Email == fb.Email.ToLowerInvariant());

            if (user == null)
            {
                var now = DateTime.UtcNow;
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = fb.Email.ToLowerInvariant(),
                    DisplayName = fb.Name ?? fb.Email,
                    ProfilePhotoUrl = fb.Picture?.Data?.Url,
                    FacebookId = fb.Id,
                    PasswordHash = _hasher.Hash(Guid.NewGuid().ToString()),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.Users.Add(user);
            }
            else
            {
                if (user.FacebookId == null) user.FacebookId = fb.Id;
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            if (!user.IsActive)
                throw new AuthException("Your account has been deactivated. Please contact support.");

            _db.AuditLogs.Add(Audit(user.UserId, "AUTH.FACEBOOK_LOGIN", ip, true));
            await _db.SaveChangesAsync();

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
            stored.IsRevoked = true;
            stored.ReplacedByToken = newRaw;

            _db.RefreshTokens.Add(new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = stored.UserId,
                Token = newRaw,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.GetRefreshTokenLifetimeDays(stored.User.RememberMe)),
                CreatedByIp = ip,
                CreatedAt = DateTime.UtcNow
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

        public async Task LogoutAsync(Guid userId, string refreshToken)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId);

            if (token != null && token.IsActive)
                token.IsRevoked = true;

            _db.AuditLogs.Add(Audit(userId, "AUTH.LOGOUT", null, true));
            await _db.SaveChangesAsync();
        }

        public async Task SendPasswordResetOtpAsync(ForgotPasswordRequest req, string? ip = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant().Trim());
            if (user == null) { await Task.Delay(200); return; }

            string otp = GenerateOtp();
            user.PasswordResetToken = _hasher.Hash(otp);
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

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
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt = DateTime.UtcNow;

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

            user.PasswordHash = _hasher.Hash(req.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

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
                UserId = user.UserId,
                Token = rawRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.GetRefreshTokenLifetimeDays(user.RememberMe)),
                CreatedByIp = ip,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            var (access, expiresIn) = _jwt.GenerateAccessToken(user);
            return ToResponse(user, access, expiresIn, rawRefresh);
        }

        private static TokenResponse ToResponse(User u, string access, int expiresIn, string refresh) => new()
        {
            AccessToken = access,
            ExpiresIn = expiresIn,
            RefreshToken = refresh,
            UserId = u.UserId,
            DisplayName = u.DisplayName,
            Email = u.Email,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            LanguagePreference = u.LanguagePreference,
            IsEmailVerified = u.IsEmailVerified,
            TwoFactorEnabled = u.TwoFactorEnabled
        };

        private static string GenerateOtp() =>
            RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        private static AuditLog Audit(Guid? uid, string type, string? ip, bool ok, string? details = null) => new()
        {
            AuditLogId = Guid.NewGuid(),
            UserId = uid,
            EventType = type,
            IpAddress = ip,
            Success = ok,
            Details = details,
            CreatedAt = DateTime.UtcNow
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
        Task SendEmailVerificationOtpAsync(string toEmail, string displayName, string otp);
        Task SendTwoFactorOtpAsync(string toEmail, string displayName, string otp);
    }

    public sealed class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
    }

    internal sealed class FacebookUserInfo
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    internal sealed class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    internal sealed class FacebookPictureData
    {
        public string? Url { get; set; }
    }
}