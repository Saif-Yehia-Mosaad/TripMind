using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

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
            e.HasOne(td => td.Trip).WithMany(t => t.TripDays)
             .HasForeignKey(td => td.TripId).OnDelete(DeleteBehavior.Cascade);
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
            e.HasOne(tl => tl.Trip).WithMany(t => t.TripLocations)
             .HasForeignKey(tl => tl.TripId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tl => tl.TripDay).WithMany(td => td.TripLocations)
             .HasForeignKey(tl => tl.TripDayId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(tl => tl.Location).WithMany(l => l.TripLocations)
             .HasForeignKey(tl => tl.LocationId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SavedItineraryConfiguration : IEntityTypeConfiguration<SavedItinerary>
    {
        public void Configure(EntityTypeBuilder<SavedItinerary> e)
        {
            e.ToTable("SavedItineraries");
            e.HasKey(si => si.SavedItineraryId);
            e.Property(si => si.SavedItineraryId).HasDefaultValueSql("NEWID()");
            e.Property(si => si.CustomName).HasMaxLength(200);
            e.Property(si => si.IsFavorite).HasDefaultValue(false);
            e.Property(si => si.SavedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(si => si.User).WithMany(u => u.SavedItineraries)
             .HasForeignKey(si => si.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(si => si.Trip).WithMany(t => t.SavedItineraries)
             .HasForeignKey(si => si.TripId).OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
    {
        public void Configure(EntityTypeBuilder<ReviewVote> e)
        {
            e.ToTable("ReviewVotes");
            e.HasKey(rv => rv.ReviewVoteId);
            e.Property(rv => rv.ReviewVoteId).HasDefaultValueSql("NEWID()");
            e.Property(rv => rv.VotedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(rv => new { rv.ReviewId, rv.UserId }).IsUnique().HasDatabaseName("UIX_ReviewVotes_Review_User");
            e.HasOne(rv => rv.Review).WithMany(r => r.ReviewVotes)
             .HasForeignKey(rv => rv.ReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rv => rv.User).WithMany(u => u.ReviewVotes)
             .HasForeignKey(rv => rv.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }

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
}
