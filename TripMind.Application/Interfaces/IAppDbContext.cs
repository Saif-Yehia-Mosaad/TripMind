using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Domain.Entities;

namespace TripMind.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<FavoritePlace> FavoritePlaces { get; }
        DbSet<FavoriteTrip> FavoriteTrips { get; }

        DbSet<Trip> Trips { get; }
        DbSet<TripReview> TripReviews { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}