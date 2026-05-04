using Microsoft.AspNetCore.Mvc;

namespace WebAdmin.Controllers
{
    /// <summary>
    /// Admin Portal Controller — handles all /admin/... routes.
    /// Each action returns its own Razor view with _AdminLayout.
    /// </summary>
    [Route("admin")]
    public class AdminController : Controller
    {
        [HttpGet("")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard() => View();

        [HttpGet("pois/pending")]
        public IActionResult PendingPois() => View();

        [HttpGet("pois/manage")]
        public IActionResult ManagePois() => View();

        [HttpGet("qr")]
        public IActionResult QrManagement() => View();

        [HttpGet("packages")]
        public IActionResult Packages() => View();

        [HttpGet("stats")]
        public IActionResult Stats() => View();

        [HttpGet("heatmap")]
        public IActionResult Heatmap() => View();

        [HttpGet("profiles")]
        public IActionResult UserProfiles() => View();

        [HttpGet("users")]
        public IActionResult Users() => View();
    }
}
