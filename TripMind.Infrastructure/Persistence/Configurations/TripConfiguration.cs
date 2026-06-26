using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> e)
        {
            e.ToTable("Trips");

            e.HasKey(t => t.TripId);
            e.Property(t => t.TripId).HasDefaultValueSql("NEWID()");

            e.Property(t => t.Title).HasMaxLength(200);
            e.Property(t => t.DestinationGovernorate).IsRequired().HasMaxLength(100);
            e.Property(t => t.SessionId).HasMaxLength(100);
            e.Property(t => t.ShareToken).HasMaxLength(64);
            e.Property(t => t.PlanJson).HasColumnType("nvarchar(max)");
            e.Property(t => t.CollectedJson).HasColumnType("nvarchar(max)");

            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            e.HasOne(t => t.User)
            .WithMany(u => u.Trips)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(t => t.ShareToken)
             .IsUnique()
             .HasFilter("[ShareToken] IS NOT NULL")
             .HasDatabaseName("UIX_Trips_ShareToken");

            e.HasIndex(t => t.SessionId)
             .HasDatabaseName("IX_Trips_SessionId");
        }
    }
}
