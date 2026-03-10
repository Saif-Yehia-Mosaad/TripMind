using System;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Budget
{
    public sealed class AllocateBudgetRequest
    {
        [Required][Range(1, 10000000)] public decimal TotalBudgetEgp { get; set; }
        // Optional user-defined weights (must sum to 1.0 if all provided)
        public decimal? AccommodationWeight { get; set; }
        public decimal? FoodWeight { get; set; }
        public decimal? TransportWeight { get; set; }
    }

    public sealed class UpdateActualSpentRequest
    {
        [Required][Range(0, 10000000)] public decimal ActualSpentEgp { get; set; }
    }

    public sealed class BudgetResponse
    {
        public Guid BudgetId { get; set; }
        public Guid TripId { get; set; }
        public decimal Total { get; set; }
        public decimal Accommodation { get; set; }
        public decimal Food { get; set; }
        public decimal Transport { get; set; }
        public decimal Activities { get; set; }
        public decimal ActualSpent { get; set; }
        public float VariancePct { get; set; }
        public string? OptimizerVersion { get; set; }
    }
}
