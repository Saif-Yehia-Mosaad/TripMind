using Microsoft.AspNetCore.Mvc;

namespace TripMind.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}