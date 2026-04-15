using Api.Modules.Localization.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Localization.Controllers;

[ApiController]
[Route("api/pois/{id}/localizations")]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpGet("{lang}")]
    public async Task<IActionResult> GetLocalization(
        int id,
        string lang,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _localizationService.GetOrCreateLocalizationAsync(
                id,
                lang,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                detail = ex.InnerException?.Message
            });
        }
    }
}