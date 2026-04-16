namespace Api.Modules.Audio.Interfaces;

public interface ITtsProvider
{
    Task<byte[]> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceCode,
        CancellationToken cancellationToken = default);
}