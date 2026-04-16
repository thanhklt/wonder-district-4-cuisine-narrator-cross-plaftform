using System.Security.Claims;
using Api.Modules.Admin.DTOs;
using Api.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Admin.Controllers;

[ApiController]
[Route("api/admin/pois")]
[Authorize(Roles = "Admin")]
public class AdminPoiController : ControllerBase
{
    private readonly IAdminPoiService _adminPoiService;

    public AdminPoiController(IAdminPoiService adminPoiService)
    {
        _adminPoiService = adminPoiService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingPois(CancellationToken cancellationToken)
    {
        var result = await _adminPoiService.GetPendingPoisAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApprovePoi(int id, CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        await _adminPoiService.ApprovePoiAsync(id, adminUserId, cancellationToken);

        return Ok(new
        {
            message = "POI approved successfully."
        });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectPoi(
        int id,
        [FromBody] RejectPoiRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        await _adminPoiService.RejectPoiAsync(id, adminUserId, request.Note, cancellationToken);

        return Ok(new
        {
            message = "POI rejected successfully."
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            throw new Exception("Invalid user token.");

        return userId;
    }
}