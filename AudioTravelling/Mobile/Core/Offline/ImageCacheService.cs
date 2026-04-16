namespace AudioTravelling.Mobile.Core.Offline;

public class ImageLocalCacheService
{
    private readonly HttpClient _httpClient;

    public ImageLocalCacheService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> DownloadImageAsync(
        int poiId,
        int imageId,
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        try
        {
            using var response = await _httpClient.GetAsync(imageUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var folder = Path.Combine(
                FileSystem.AppDataDirectory,
                "cache",
                "images",
                "pois",
                poiId.ToString());

            Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            var fileName = $"poi_image_{imageId}{extension}";
            var filePath = Path.Combine(folder, fileName);

            await using var inputStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var outputStream = File.Create(filePath);
            await inputStream.CopyToAsync(outputStream, cancellationToken);

            return filePath;
        }
        catch
        {
            return null;
        }
    }
}