using TripMind.Application.Services;

namespace TripMind.Infrastructure.Security
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;
        public string Hash(string plaintext) => BCrypt.Net.BCrypt.HashPassword(plaintext, WorkFactor);
        public bool Verify(string plaintext, string hash) => BCrypt.Net.BCrypt.Verify(plaintext, hash);
    }
}
