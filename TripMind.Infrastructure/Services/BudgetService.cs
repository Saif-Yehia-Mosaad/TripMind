using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripMind.Application.DTOs.Budget;
using TripMind.Domain.Entities;
using TripMind.Infrastructure.Persistence;

namespace TripMind.Infrastructure.Services
{
    public sealed class BudgetService
    {
        private readonly TripMindDbContext _db;
        public BudgetService(TripMindDbContext db) => _db = db;

        /// <summary>
        /// Creates or replaces the budget for a trip using simple ratio-based allocation.
        /// In production this delegates to the Python AI Engine (linear programming).
        /// Ratios: Accommodation 40%, Food 25%, Transport 20%, Activities 15%.
        /// </summary>
        public async Task<BudgetResponse> AllocateBudgetAsync(Guid userId, Guid tripId, AllocateBudgetRequest req)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId)
                ?? throw new KeyNotFoundException("Trip not found.");

            decimal total = req.TotalBudgetEgp;

            // Proportional split — override with user weights if provided
            decimal accommodation = Math.Round(total * (req.AccommodationWeight ?? 0.40m), 2);
            decimal food          = Math.Round(total * (req.FoodWeight          ?? 0.25m), 2);
            decimal transport     = Math.Round(total * (req.TransportWeight     ?? 0.20m), 2);
            decimal activities    = total - accommodation - food - transport; // remainder

            // Upsert budget record
            var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.TripId == tripId);
            if (budget == null)
            {
                budget = new Budget { BudgetId = Guid.NewGuid(), TripId = tripId, CreatedAt = DateTime.UtcNow };
                _db.Budgets.Add(budget);
            }

            budget.TotalBudgetEgp             = total;
            budget.AccommodationAllocationEgp  = accommodation;
            budget.FoodAllocationEgp           = food;
            budget.TransportAllocationEgp      = transport;
            budget.ActivitiesAllocationEgp     = activities;
            budget.OptimizerVersion            = "v1.0-ratio";
            budget.UpdatedAt                   = DateTime.UtcNow;

            trip.TotalBudgetEgp = total;
            await _db.SaveChangesAsync();

            return MapToResponse(budget);
        }

        public async Task<BudgetResponse> GetBudgetAsync(Guid userId, Guid tripId)
        {
            // Verify trip ownership first
            var exists = await _db.Trips.AnyAsync(t => t.TripId == tripId && t.UserId == userId);
            if (!exists) throw new KeyNotFoundException("Trip not found.");

            var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.TripId == tripId)
                ?? throw new KeyNotFoundException("Budget not yet generated for this trip.");

            return MapToResponse(budget);
        }

        public async Task<BudgetResponse> UpdateActualSpentAsync(Guid userId, Guid tripId, decimal actualSpent)
        {
            var exists = await _db.Trips.AnyAsync(t => t.TripId == tripId && t.UserId == userId);
            if (!exists) throw new KeyNotFoundException("Trip not found.");

            var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.TripId == tripId)
                ?? throw new KeyNotFoundException("No budget found.");

            budget.ActualSpentEgp    = actualSpent;
            budget.BudgetVariancePct = budget.TotalBudgetEgp == 0 ? 0
                : (float)((actualSpent - budget.TotalBudgetEgp) / budget.TotalBudgetEgp * 100);
            budget.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return MapToResponse(budget);
        }

        private static BudgetResponse MapToResponse(Budget b) => new()
        {
            BudgetId       = b.BudgetId,
            TripId         = b.TripId,
            Total          = b.TotalBudgetEgp,
            Accommodation  = b.AccommodationAllocationEgp,
            Food           = b.FoodAllocationEgp,
            Transport      = b.TransportAllocationEgp,
            Activities     = b.ActivitiesAllocationEgp,
            ActualSpent    = b.ActualSpentEgp,
            VariancePct    = b.BudgetVariancePct,
            OptimizerVersion = b.OptimizerVersion
        };
    }
}
