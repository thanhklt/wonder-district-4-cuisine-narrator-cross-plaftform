namespace AudioTravelling.Mobile.Core.Offline;

/// <summary>
/// Resolves localized strings from the local cache when the device is offline.
/// Falls back to a default language if the requested language is not cached.
/// </summary>
public class OfflineLocalizationResolver
{
    private readonly string _defaultLanguage;

    public OfflineLocalizationResolver(string defaultLanguage = "vi-VN")
    {
        _defaultLanguage = defaultLanguage;
    }

    /// <summary>
    /// Resolves a localized string by key and language code.
    /// </summary>
    public Task<string> ResolveAsync(string key, string? languageCode = null)
    {
        // TODO: Implement offline localization resolution from local cache
        throw new NotImplementedException();
    }
}
