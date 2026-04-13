using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class TourPackageConfiguration : IEntityTypeConfiguration<TourPackage>
    {
        public void Configure(EntityTypeBuilder<TourPackage> e)
        {
            e.ToTable("TourPackages");
            e.HasKey(t => t.TourPackageId);
            e.Property(t => t.TourPackageId).HasDefaultValueSql("NEWID()");
            e.Property(t => t.NameEn).IsRequired().HasMaxLength(200);
            e.Property(t => t.NameAr).IsRequired().HasMaxLength(200);
            e.Property(t => t.Governorate).IsRequired().HasMaxLength(100);
            e.Property(t => t.PricePerPersonEgp).HasColumnType("decimal(10,2)");
            e.Property(t => t.PhotoUrl).HasMaxLength(2048);
            e.Property(t => t.IsActive).HasDefaultValue(true);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }

    public class TourPackageLocationConfiguration : IEntityTypeConfiguration<TourPackageLocation>
    {
        public void Configure(EntityTypeBuilder<TourPackageLocation> e)
        {
            e.ToTable("TourPackageLocations");
            e.HasKey(t => t.TourPackageLocationId);
            e.Property(t => t.TourPackageLocationId).HasDefaultValueSql("NEWID()");
            e.HasOne(t => t.TourPackage)
             .WithMany(p => p.Locations)
             .HasForeignKey(t => t.TourPackageId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Location)
             .WithMany()
             .HasForeignKey(t => t.LocationId)
             .OnDelete(DeleteBehavior.NoAction);
        }
    }
}