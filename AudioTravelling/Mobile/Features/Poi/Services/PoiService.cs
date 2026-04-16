
namespace AudioTravelling.Mobile.Features.Poi.Services;

/// <summary>
/// Shared POI-related app state.
/// Hiện tại chỉ giữ cờ bật/tắt audio toàn cục.
/// Dữ liệu POI sẽ được lấy từ API, không còn dùng dummy data nữa.
/// </summary>
public static class PoiService
{
    private static bool _audioEnabled = true;

    public static bool AudioEnabled
    {
        get => _audioEnabled;
        set
        {
            if (_audioEnabled == value) return;
            _audioEnabled = value;
            AudioEnabledChanged?.Invoke(value);
        }
    }

    public static event Action<bool>? AudioEnabledChanged;
}