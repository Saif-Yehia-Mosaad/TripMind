using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> e)
        {
            e.ToTable("Reviews");
            e.HasKey(r => r.ReviewId);
            e.Property(r => r.ReviewId).HasDefaultValueSql("NEWID()");
            e.Property(r => r.Rating).IsRequired();
            e.ToTable("Reviews", t =>
            {
                t.HasCheckConstraint(
                    "CK_Reviews_Rating",
                    "[Rating] BETWEEN 1 AND 5");
            });
            e.Property(r => r.ReviewText).HasColumnType("nvarchar(max)");
            e.Property(r => r.PhotoUrl).HasMaxLength(2048);
            e.Property(r => r.HelpfulCount).HasDefaultValue(0);
            e.Property(r => r.Reported).HasDefaultValue(false);
            e.Property(r => r.ModerationStatus).HasConversion<string>();
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(r => new { r.UserId, r.LocationId }).IsUnique().HasDatabaseName("UIX_Reviews_User_Location");
            e.HasOne(r => r.User).WithMany(u => u.Reviews)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Location).WithMany(l => l.Reviews)
             .HasForeignKey(r => r.LocationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
