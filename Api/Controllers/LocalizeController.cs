using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.DTOs;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizeController(LocalizeService service) : ControllerBase
    {
        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            var res = await service.TranslateAsync(request);
            if (res == null) return BadRequest();
            return Ok(res);
        }
    }
}
