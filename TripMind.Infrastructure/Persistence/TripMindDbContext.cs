using Microsoft.EntityFrameworkCore;
using TripMind.Application.Interfaces;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence
{
    public sealed class TripMindDbContext : DbContext, IAppDbContext
    {
        public TripMindDbContext(DbContextOptions<TripMindDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<TripDay> TripDays { get; set; } = null!;
        public DbSet<TripLocation> TripLocations { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<UserInterest> UserInterests { get; set; } = null!;
        public DbSet<UserPreference> UserPreferences { get; set; } = null!;
        public DbSet<LocationFeature> LocationFeatures { get; set; } = null!;
        public DbSet<HiddenGem> HiddenGems { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<TourPackage> TourPackages { get; set; } = null!;
        public DbSet<TourPackageLocation> TourPackageLocations { get; set; } = null!;
        public DbSet<FavoritePlace> FavoritePlaces { get; set; } = null!;
        public DbSet<FavoriteTrip> FavoriteTrips { get; set; } = null!;
        public DbSet<TripReview> TripReviews { get; set; } = null!;

        DbSet<User> IAppDbContext.Users => Users;
        DbSet<AuditLog> IAppDbContext.AuditLogs => AuditLogs;
        DbSet<RefreshToken> IAppDbContext.RefreshTokens => RefreshTokens;

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);
            m.ApplyConfigurationsFromAssembly(typeof(TripMindDbContext).Assembly);
        }
    }
}