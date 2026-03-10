using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> e)
        {
            e.ToTable("Users");
            e.HasKey(u => u.UserId);
            e.Property(u => u.UserId).HasDefaultValueSql("NEWID()");
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UIX_Users_Email");
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            e.Property(u => u.ProfilePhotoUrl).HasMaxLength(2048);
            e.Property(u => u.HomeGovernorate).HasMaxLength(100);
            e.Property(u => u.LanguagePreference).HasMaxLength(2).HasDefaultValue("AR");
            e.Property(u => u.RememberMe).HasDefaultValue(false);
            e.Property(u => u.GoogleId).HasMaxLength(128);
            e.Property(u => u.FacebookId).HasMaxLength(128);
            e.Property(u => u.PasswordResetToken).HasMaxLength(512);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
