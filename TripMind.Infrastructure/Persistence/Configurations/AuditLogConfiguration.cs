using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> e)
        {
            e.ToTable("AuditLogs");
            e.HasKey(a => a.AuditLogId);
            e.Property(a => a.AuditLogId).HasDefaultValueSql("NEWID()");
            e.Property(a => a.EventType).IsRequired().HasMaxLength(100);
            e.Property(a => a.IpAddress).HasMaxLength(45);
            e.Property(a => a.UserAgent).HasMaxLength(512);
            e.Property(a => a.Details).HasColumnType("nvarchar(max)");
            e.Property(a => a.Success).HasDefaultValue(true);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(a => a.UserId).HasDatabaseName("IX_AuditLogs_UserId");
            e.HasIndex(a => new { a.EventType, a.CreatedAt }).HasDatabaseName("IX_AuditLogs_EventType_Created");
            e.HasOne(a => a.User).WithMany(u => u.AuditLogs)
             .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
