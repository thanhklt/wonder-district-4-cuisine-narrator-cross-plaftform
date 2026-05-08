using Microsoft.AspNetCore.Mvc;

namespace WebAdmin.Controllers
{
    /// <summary>
    /// Owner Portal Controller — handles all /owner/... routes.
    /// Each action returns its own Razor view with _OwnerLayout.
    /// </summary>
    [Route("owner")]
    public class OwnerController : Controller
    {
        [HttpGet("")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard() => View();

        [HttpGet("pois")]
        public IActionResult MyPois() => View();

        [HttpGet("pois/create")]
        public IActionResult CreatePoi() => View();

        [HttpGet("pois/edit/{id:int}")]
        public IActionResult EditPoi(int id) => View();

        [HttpGet("pois/status")]
        public IActionResult PoiStatus() => View();

    }
}
