using AudioTravelling.API.DTOs;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/pois")]
public class LocalizationController(IAppDbContext db, ILocalizationService localization) : ControllerBase
{
    [HttpPost("{id:guid}/localize")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Localize(Guid id)
    {
        var exists = await db.Pois.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();
        await localization.LocalizePoiAsync(id);
        return Ok(new MessageResponse("Localization triggered"));
    }

    [HttpGet("{id:guid}/audio")]
    public async Task<IActionResult> GetAudio(Guid id, [FromQuery] string lang = "vi")
    {
        var loc = await db.PoiLocalizations
            .FirstOrDefaultAsync(l => l.PoiId == id && l.Language == lang);

        if (loc?.AudioUrl is null)
        {
            // Fallback to English
            loc = await db.PoiLocalizations
                .FirstOrDefaultAsync(l => l.PoiId == id && l.Language == "en");
        }

        if (loc?.AudioUrl is null) return NotFound();
        return Ok(new AudioResponse(loc.AudioUrl));
    }
}
