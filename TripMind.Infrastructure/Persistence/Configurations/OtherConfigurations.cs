using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;
using TripMind.Domain.Enums;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class TripDayConfiguration : IEntityTypeConfiguration<TripDay>
    {
        public void Configure(EntityTypeBuilder<TripDay> e)
        {
            e.ToTable("TripDays");
            e.HasKey(td => td.TripDayId);
            e.Property(td => td.TripDayId).HasDefaultValueSql("NEWID()");
            e.Property(td => td.Notes).HasColumnType("nvarchar(max)");
        }
    }

    public class TripLocationConfiguration : IEntityTypeConfiguration<TripLocation>
    {
        public void Configure(EntityTypeBuilder<TripLocation> e)
        {
            e.ToTable("TripLocations");
            e.HasKey(tl => tl.TripLocationId);
            e.Property(tl => tl.TripLocationId).HasDefaultValueSql("NEWID()");
            e.Property(tl => tl.TimeSlot).IsRequired().HasMaxLength(5);
            e.HasOne(tl => tl.TripDay).WithMany(td => td.TripLocations)
             .HasForeignKey(tl => tl.TripDayId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(tl => tl.Location).WithMany(l => l.TripLocations)
             .HasForeignKey(tl => tl.LocationId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Matches the REAL SavedItinerary entity: Title/City/Days/People/Budget/
    // TotalCost/PlanJson/ShareToken/CreatedAt — a saved AI-generated plan.
    // It does NOT reference Trip — that's a separate concept (FavoriteTrip).

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

    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> e)
        {
            e.ToTable("UserPreferences");
            e.HasKey(up => up.UserPreferenceId);
            e.Property(up => up.UserPreferenceId).HasDefaultValueSql("NEWID()");
            e.Property(up => up.PreferenceKey).IsRequired().HasMaxLength(100);
            e.Property(up => up.PreferenceValue).IsRequired().HasMaxLength(500);
            e.Property(up => up.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(up => up.User).WithMany(u => u.UserPreferences)
             .HasForeignKey(up => up.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class LocationFeatureConfiguration : IEntityTypeConfiguration<LocationFeature>
    {
        public void Configure(EntityTypeBuilder<LocationFeature> e)
        {
            e.ToTable("LocationFeatures");
            e.HasKey(lf => lf.LocationFeatureId);
            e.Property(lf => lf.LocationFeatureId).HasDefaultValueSql("NEWID()");
            e.Property(lf => lf.FeatureKey).IsRequired().HasMaxLength(100);
            e.HasOne(lf => lf.Location).WithMany(l => l.LocationFeatures)
             .HasForeignKey(lf => lf.LocationId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class HiddenGemConfiguration : IEntityTypeConfiguration<HiddenGem>
    {
        public void Configure(EntityTypeBuilder<HiddenGem> e)
        {
            e.ToTable("HiddenGems");
            e.HasKey(hg => hg.HiddenGemId);
            e.Property(hg => hg.HiddenGemId).HasDefaultValueSql("NEWID()");
            e.Property(hg => hg.Story).HasColumnType("nvarchar(max)");
            e.Property(hg => hg.TaggedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(hg => hg.Location).WithOne(l => l.HiddenGem)
             .HasForeignKey<HiddenGem>(hg => hg.LocationId).OnDelete(DeleteBehavior.Cascade);
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
            e.Property(f => f.Name).IsRequired().HasMaxLength(300);
            e.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(f => f.User).WithMany()
             .HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
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
            e.HasOne(f => f.User).WithMany()
             .HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(f => f.Trip).WithMany()
             .HasForeignKey(f => f.TripId).OnDelete(DeleteBehavior.Cascade);
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
            e.HasOne(r => r.Trip).WithMany()
             .HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.User).WithMany()
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(r => new { r.TripId, r.UserId }).IsUnique()
             .HasDatabaseName("UIX_TripReviews_TripUser");
        }
    }
}