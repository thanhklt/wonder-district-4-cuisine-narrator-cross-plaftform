using System.Globalization;

namespace AudioTravelling.Mobile.Services.Audio;

public class AudioTriggerService : IAudioTriggerService
{
    private readonly AudioFacadeService _audioFacadeService;

    // 🔥 chống spam audio (geofence)
    private int? _lastPoiId;
    private DateTime _lastTriggeredAt = DateTime.MinValue;

    private static readonly TimeSpan ShortCooldown = TimeSpan.FromSeconds(45);

    public AudioTriggerService(AudioFacadeService audioFacadeService)
    {
        _audioFacadeService = audioFacadeService;
    }

    public async Task TriggerAsync(int poiId, string lang)
    {
        await _audioFacadeService.PlayPoiAudioAsync(poiId, lang);
    }

    public async Task TriggerGeofenceAudioAsync(
        int poiId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // 🚫 Nếu cùng POI và chưa qua cooldown → bỏ qua
        if (_lastPoiId == poiId &&
            now - _lastTriggeredAt < ShortCooldown)
        {
            return;
        }

        _lastPoiId = poiId;
        _lastTriggeredAt = now;

        // 🌍 lấy ngôn ngữ thiết bị
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        await _audioFacadeService.PlayPoiAudioAsync(
            poiId,
            lang,
            cancellationToken);
    }
}