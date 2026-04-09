namespace AudioTravelling.Mobile.Features.Audio.Services;

/// <summary>
/// Provides Text-to-Speech functionality using the platform's native TTS engine.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    /// Speaks the provided text aloud using the platform's native TTS engine.
    /// </summary>
    /// <param name="text">The text content to be spoken.</param>
    /// <param name="languageCode">
    /// An optional BCP-47 language code (e.g., "en-US", "vi-VN").
    /// If provided, the service will attempt to match a locale/voice for that language.
    /// If null or no match is found, the device default locale is used.
    /// </param>
    Task SpeakAsync(string text, string languageCode = null);

    /// <summary>
    /// Cancels any currently active TTS playback.
    /// </summary>
    void CancelPlayback();
}
