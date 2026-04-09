namespace AudioTravelling.Mobile.Features.Audio.Services;

/// <summary>
/// Implementation of <see cref="ITextToSpeechService"/> using MAUI's built-in
/// <see cref="Microsoft.Maui.Media.ITextToSpeech"/> API.
/// </summary>
public class TextToSpeechService : ITextToSpeechService
{
    private CancellationTokenSource _cancellationTokenSource;

    /// <inheritdoc />
    public async Task SpeakAsync(string text, string languageCode = null)
    {
        // Cancel any previous playback before starting a new one
        CancelPlayback();

        _cancellationTokenSource = new CancellationTokenSource();

        var options = new SpeechOptions();

        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Try exact match first (e.g., "en-US"), then fall back to language-only match (e.g., "en")
            var matchedLocale = locales.FirstOrDefault(l =>
                    l.Language.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                ?? locales.FirstOrDefault(l =>
                    l.Language.StartsWith(
                        languageCode.Split('-')[0],
                        StringComparison.OrdinalIgnoreCase));

            if (matchedLocale != null)
            {
                options.Locale = matchedLocale;
            }
        }

        await TextToSpeech.Default.SpeakAsync(text, options, _cancellationTokenSource.Token);
    }

    /// <inheritdoc />
    public void CancelPlayback()
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
