using Microsoft.AspNetCore.Hosting;

namespace Api.Modules.Audio.Services;

public class AudioFileService
{
    private readonly IWebHostEnvironment _env;

    public AudioFileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SavePoiAudioAsync(
        int poiId,
        string lang,
        byte[] audioBytes,
        CancellationToken cancellationToken = default)
    {
        var folder = Path.Combine(
            _env.WebRootPath,
            "uploads",
            "audio",
            "pois",
            poiId.ToString());

        Directory.CreateDirectory(folder);

        var fileName = $"{lang}.mp3";
        var fullPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(fullPath, audioBytes, cancellationToken);

        return $"/uploads/audio/pois/{poiId}/{fileName}";
    }

    public bool AudioExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var cleanPath = relativePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(_env.WebRootPath, cleanPath);
        return File.Exists(fullPath);
    }
}