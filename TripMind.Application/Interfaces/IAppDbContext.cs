using Microsoft.EntityFrameworkCore;
using TripMind.Domain.Entities;

namespace TripMind.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}
