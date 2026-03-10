using System;

namespace TripMind.Domain.Entities
{
    public class Budget
    {
        public Guid BudgetId { get; set; }
        public Guid TripId { get; set; }
        public decimal TotalBudgetEgp { get; set; }
        public decimal AccommodationAllocationEgp { get; set; }
        public decimal FoodAllocationEgp { get; set; }
        public decimal TransportAllocationEgp { get; set; }
        public decimal ActivitiesAllocationEgp { get; set; }
        public decimal ActualSpentEgp { get; set; }
        public float BudgetVariancePct { get; set; }
        public string? OptimizerVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Trip Trip { get; set; } = null!;
    }
}
