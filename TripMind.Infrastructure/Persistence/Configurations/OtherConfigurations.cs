using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;
using TripMind.Domain.Enums;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
    {
        public void Configure(EntityTypeBuilder<UserInterest> e)
        {
            e.ToTable("UserInterests");
            e.HasKey(ui => ui.UserInterestId);
            e.Property(ui => ui.UserInterestId).HasDefaultValueSql("NEWID()");
            e.Property(ui => ui.InterestTag).IsRequired().HasMaxLength(100);
            e.Property(ui => ui.Weight).HasDefaultValue(1);
            e.HasOne(ui => ui.User).WithMany(u => u.UserInterests)
             .HasForeignKey(ui => ui.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class FavoritePlaceConfiguration : IEntityTypeConfiguration<FavoritePlace>
    {
        public void Configure(EntityTypeBuilder<FavoritePlace> e)
        {
            e.ToTable("FavoritePlaces");
            e.HasKey(f => f.FavoritePlaceId);
            e.Property(f => f.FavoritePlaceId).HasDefaultValueSql("NEWID()");
            e.Property(f => f.PlaceId).IsRequired().HasMaxLength(200);
            e.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            // NOTE: matches the FK as it actually exists in the database (NoAction).
            // FavoritePlace rows are deleted manually in UserService.DeleteAccountAsync
            // before the user row is removed - same pattern as FavoriteTrip/TripReview below.
            // Do NOT change this back to Cascade without also generating a real EF migration
            // (see CheckModelChanges / FixUserDeleteCascade migration history - the DB and
            // the old version of this file had drifted apart).
            e.HasOne(f => f.User)
             .WithMany()
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(f => new { f.UserId, f.PlaceId }).IsUnique()
             .HasDatabaseName("UIX_FavoritePlaces_UserPlace");
        }
    }

    public class FavoriteTripConfiguration : IEntityTypeConfiguration<FavoriteTrip>
    {
        public void Configure(EntityTypeBuilder<FavoriteTrip> e)
        {
            e.ToTable("FavoriteTrips");
            e.HasKey(f => f.FavoriteTripId);
            e.Property(f => f.FavoriteTripId).HasDefaultValueSql("NEWID()");
            e.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(f => f.User)
             .WithMany()
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(f => f.Trip)
             .WithMany()
             .HasForeignKey(f => f.TripId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(f => new { f.UserId, f.TripId }).IsUnique()
             .HasDatabaseName("UIX_FavoriteTrips_UserTrip");
        }
    }

    public class TripReviewConfiguration : IEntityTypeConfiguration<TripReview>
    {
        public void Configure(EntityTypeBuilder<TripReview> e)
        {
            e.ToTable("TripReviews");
            e.HasKey(r => r.TripReviewId);
            e.Property(r => r.TripReviewId).HasDefaultValueSql("NEWID()");
            e.Property(r => r.Rating).IsRequired();
            e.Property(r => r.Comment).HasMaxLength(1000);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(r => r.Trip)
            .WithMany(t => t.TripReviews)
            .HasForeignKey(r => r.TripId)
            .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.User)
  .WithMany()
  .HasForeignKey(r => r.UserId)
  .OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(r => new { r.TripId, r.UserId }).IsUnique()
             .HasDatabaseName("UIX_TripReviews_TripUser");
        }
    }
}
