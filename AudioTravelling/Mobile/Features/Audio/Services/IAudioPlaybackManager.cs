using AudioTravelling.Mobile.Features.Audio.Models;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public interface IAudioPlaybackManager
{
    Task RequestPlayAsync(AudioPlayRequest request);
    void StopAndClear();
    int? CurrentPlayingPoiId { get; }
}
