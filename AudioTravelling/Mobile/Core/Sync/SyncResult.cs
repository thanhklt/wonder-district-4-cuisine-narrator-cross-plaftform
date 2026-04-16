namespace AudioTravelling.Mobile.Core.Sync;

public class SyncResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public int PoiCount { get; set; }
    public int ImageCount { get; set; }
    public int LocalizationCount { get; set; }
    public int AudioCount { get; set; }

    public static SyncResult Success(
        int poiCount,
        int imageCount,
        int localizationCount,
        int audioCount)
    {
        return new SyncResult
        {
            IsSuccess = true,
            PoiCount = poiCount,
            ImageCount = imageCount,
            LocalizationCount = localizationCount,
            AudioCount = audioCount
        };
    }

    public static SyncResult Failure(string errorMessage)
    {
        return new SyncResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}