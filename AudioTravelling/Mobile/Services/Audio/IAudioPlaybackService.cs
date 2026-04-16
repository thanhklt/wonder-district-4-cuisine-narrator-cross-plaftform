namespace AudioTravelling.Mobile.Services.Audio;

public interface IAudioPlaybackService
{
    Task PlayRemoteAsync(string url, CancellationToken cancellationToken = default);
    Task PlayLocalAsync(string localFilePath, CancellationToken cancellationToken = default);
    Task StopAsync();
}