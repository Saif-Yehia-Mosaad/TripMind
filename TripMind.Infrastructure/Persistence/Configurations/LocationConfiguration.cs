using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> e)
        {
            e.ToTable("Locations");
            e.HasKey(l => l.LocationId);
            e.Property(l => l.LocationId).HasDefaultValueSql("NEWID()");
            e.Property(l => l.NameAr).IsRequired().HasMaxLength(200);
            e.Property(l => l.NameEn).IsRequired().HasMaxLength(200);
            e.Property(l => l.Category).HasConversion<string>();
            e.Property(l => l.Governorate).IsRequired().HasMaxLength(100);
            e.Property(l => l.DescriptionAr).HasColumnType("nvarchar(max)");
            e.Property(l => l.DescriptionEn).HasColumnType("nvarchar(max)");
            e.Property(l => l.EntryFeeEgp).HasColumnType("decimal(10,2)");
            e.Property(l => l.AvgPricePerNightEgp).HasColumnType("decimal(10,2)");
            e.Property(l => l.AvgMealPriceEgp).HasColumnType("decimal(10,2)");
            e.Property(l => l.OpeningHours).HasMaxLength(200);
            e.Property(l => l.IsHiddenGem).HasDefaultValue(false);
            e.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(l => l.Governorate).HasDatabaseName("IX_Locations_Governorate");
            e.HasIndex(l => l.Category).HasDatabaseName("IX_Locations_Category");
        }
    }
}
