namespace Api.Modules.Owner.Controllers;

using Api.Modules.Owner.Interfaces;
using Api.Modules.Owner.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/owner")]
public class OwnerAuthController : ControllerBase
{
    private readonly IOwnerService _ownerService;

    public OwnerAuthController(IOwnerService ownerService)
    {
        _ownerService = ownerService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(OwnerSignupRequest request)
    {
        var result = await _ownerService.SignupAsync(request);
        return Ok(result);
    }
}