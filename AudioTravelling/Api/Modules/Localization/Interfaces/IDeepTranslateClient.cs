namespace Api.Modules.Localization.Interfaces;

public interface IDeepTranslateClient
{
    Task<string> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}