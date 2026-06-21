using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripMind.Domain.Entities;

namespace TripMind.Infrastructure.Persistence.Configurations
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> e)
        {
            e.ToTable("Budgets");
            e.HasKey(b => b.BudgetId);
            e.Property(b => b.BudgetId).HasDefaultValueSql("NEWID()");
            e.Property(b => b.TotalBudgetEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.AccommodationAllocationEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.FoodAllocationEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.TransportAllocationEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.ActivitiesAllocationEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.ActualSpentEgp).HasColumnType("decimal(12,2)");
            e.Property(b => b.OptimizerVersion).HasMaxLength(20);
            e.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(b => b.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
