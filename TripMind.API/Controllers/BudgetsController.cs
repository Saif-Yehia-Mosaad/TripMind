using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Budget;
using TripMind.Application.Services;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/trips/{tripId:guid}/budget")]
    [Authorize]
    public sealed class BudgetsController : ControllerBase
    {
        private readonly BudgetService _budgets;
        public BudgetsController(BudgetService budgets) => _budgets = budgets;

        private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get(Guid tripId)
            => Ok(await _budgets.GetBudgetAsync(Me, tripId));

        [HttpPost]
        public async Task<IActionResult> Allocate(Guid tripId, [FromBody] AllocateBudgetRequest req)
            => Ok(await _budgets.AllocateBudgetAsync(Me, tripId, req));

        [HttpPatch("actual-spent")]
        public async Task<IActionResult> UpdateActualSpent(Guid tripId, [FromBody] UpdateActualSpentRequest req)
            => Ok(await _budgets.UpdateActualSpentAsync(Me, tripId, req.ActualSpentEgp));
    }
}
