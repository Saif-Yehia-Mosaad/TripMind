using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> e)
        {
            e.ToTable("RefreshTokens");
            e.HasKey(r => r.RefreshTokenId);
            e.Property(r => r.RefreshTokenId).HasDefaultValueSql("NEWID()");
            e.Property(r => r.Token).IsRequired().HasMaxLength(512);
            e.Property(r => r.ReplacedByToken).HasMaxLength(512);
            e.Property(r => r.CreatedByIp).HasMaxLength(45);
            e.Property(r => r.IsRevoked).HasDefaultValue(false);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(r => r.Token).IsUnique().HasDatabaseName("UIX_RefreshTokens_Token");
            e.HasIndex(r => r.UserId).HasDatabaseName("IX_RefreshTokens_UserId");
            e.Ignore(r => r.IsActive);
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
