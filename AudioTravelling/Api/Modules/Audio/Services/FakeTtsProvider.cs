using System.Text;
using Api.Modules.Audio.Interfaces;

namespace Api.Modules.Audio.Services;

public class FakeTtsProvider : ITtsProvider
{
    public Task<byte[]> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceCode,
        CancellationToken cancellationToken = default)
    {
        // Fake bytes để test flow lưu file.
        // Lưu ý: file này không phải mp3 thật.
        // Chỉ dùng để test backend flow.
        var fakeContent = $"FAKE AUDIO | lang={languageCode} | voice={voiceCode} | text={text}";
        return Task.FromResult(Encoding.UTF8.GetBytes(fakeContent));
    }
}