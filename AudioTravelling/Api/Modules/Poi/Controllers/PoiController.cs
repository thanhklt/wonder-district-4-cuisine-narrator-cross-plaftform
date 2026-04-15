using Api.Modules.Poi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Poi.Controllers;

[ApiController]
[Route("api/pois")]
public class PoiController : ControllerBase
{
    private readonly IPoiService _poiService;

    public PoiController(IPoiService poiService)
    {
        _poiService = poiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pois = await _poiService.GetActivePoisAsync();
        return Ok(pois);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var poi = await _poiService.GetPoiByIdAsync(id);

        if (poi == null)
            return NotFound(new { message = "POI not found." });

        return Ok(poi);
    }
}