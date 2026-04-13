using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TripMind.Application.DTOs.Location;
using TripMind.Application.DTOs.TourPackage;
using TripMind.Infrastructure.Services;

namespace TripMind.API.Controllers
{
    [ApiController]
    [Route("api/v1/tour-packages")]
    [Produces("application/json")]
    public sealed class TourPackagesController : ControllerBase
    {
        private readonly TourPackageService _packages;
        public TourPackagesController(TourPackageService packages) => _packages = packages;

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TourPackageResponse>), 200)]
        public async Task<IActionResult> Search([FromQuery] TourPackageSearchRequest req) =>
            Ok(await _packages.SearchAsync(req));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TourPackageResponse), 200)]
        public async Task<IActionResult> GetById(Guid id) =>
            Ok(await _packages.GetByIdAsync(id));
    }
}