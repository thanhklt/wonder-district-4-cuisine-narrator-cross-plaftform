using Api.Modules.Audio.DTOs;

namespace Api.Modules.Audio.Interfaces;

public interface IAudioService
{
    Task<AudioResponse> GetPoiAudioAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default);
}