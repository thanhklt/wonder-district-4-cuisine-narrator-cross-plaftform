using Microsoft.AspNetCore.Mvc;
using Api.Services;


namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PoiController : ControllerBase
    {
        private PoiService _service;
        public PoiController(PoiService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await _service.GetAllAsync();
            return Ok(res);
        }
    }
}
