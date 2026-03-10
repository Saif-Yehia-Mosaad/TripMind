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
            e.Property(t => t.DestinationGovernorate).IsRequired().HasMaxLength(100);
            e.Property(t => t.TotalBudgetEgp).HasColumnType("decimal(12,2)");
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.IsPublic).HasDefaultValue(false);
            e.Property(t => t.ShareToken).IsRequired().HasMaxLength(64);
            e.HasIndex(t => t.ShareToken).IsUnique().HasDatabaseName("UIX_Trips_ShareToken");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(t => t.User).WithMany(u => u.Trips)
             .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
