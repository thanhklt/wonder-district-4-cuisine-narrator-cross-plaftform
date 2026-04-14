namespace Api.Modules.Owner.Controllers;

using System.Security.Claims;
using Api.Modules.Owner.DTOs;
using Api.Modules.Owner.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/owner/pois")]
[Authorize(Roles = "Owner")]
//[Authorize]
public class OwnerPoiController : ControllerBase
{
    private readonly IOwnerPoiService _ownerPoiService;

    public OwnerPoiController(IOwnerPoiService poiService)
    {
        _ownerPoiService = poiService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePoi(CreatePoiRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        int ownerId = int.Parse(userIdClaim);

        var result = await _ownerPoiService.CreatePoiAsync(request, ownerId);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPois()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        int ownerId = int.Parse(userIdClaim);

        var result = await _ownerPoiService.GetMyPoisAsync(ownerId);

        return Ok(result);
    }
}