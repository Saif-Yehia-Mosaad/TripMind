using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Security
{
    public sealed class JwtProvider : IJwtProvider
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private const int AccessTokenMinutes = 15;

        public JwtProvider(IConfiguration config)
        {
            _secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            _issuer = config["Jwt:Issuer"] ?? "TripMind";
            _audience = config["Jwt:Audience"] ?? "TripMindUsers";
        }

        public (string Token, int ExpiresInSeconds) GenerateAccessToken(User user)
        {
            var expiry = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name,  user.DisplayName),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new("lang",        user.LanguagePreference),
                new("governorate", user.HomeGovernorate ?? string.Empty),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_issuer, _audience, claims,
                notBefore: DateTime.UtcNow, expires: expiry, signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), AccessTokenMinutes * 60);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public int GetRefreshTokenLifetimeDays(bool rememberMe) => rememberMe ? 30 : 7;
    }
}