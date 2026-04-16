using Api.Modules.Audio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Audio.Controllers;

[ApiController]
[Route("api/pois")]
public class AudioController : ControllerBase
{
    private readonly IAudioService _audioService;

    public AudioController(IAudioService audioService)
    {
        _audioService = audioService;
    }

    [HttpGet("{id}/audio")]
    public async Task<IActionResult> GetAudio(
        int id,
        [FromQuery] string lang = "vi",
        CancellationToken cancellationToken = default)
    {
        var result = await _audioService.GetPoiAudioAsync(id, lang, cancellationToken);
        return Ok(result);
    }
}