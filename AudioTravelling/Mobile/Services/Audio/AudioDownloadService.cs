namespace AudioTravelling.Mobile.Services.Audio;

public class AudioDownloadService
{
    private readonly HttpClient _httpClient;

    public AudioDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> DownloadAudioAsync(
        string audioUrl,
        int poiId,
        string lang,
        CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(
            FileSystem.AppDataDirectory,
            "audio",
            "pois",
            poiId.ToString());

        Directory.CreateDirectory(folder);

        var localPath = Path.Combine(folder, $"{lang}.mp3");

        using var stream = await _httpClient.GetStreamAsync(audioUrl, cancellationToken);
        await using var fileStream = File.Create(localPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return localPath;
    }
}