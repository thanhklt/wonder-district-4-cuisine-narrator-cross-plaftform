#if ANDROID
using Android.App;
using Android.Content;
#endif

using Mobile.Models;

namespace Mobile.Services;

public class DeviceConfigService
{
    private DeviceProfile? _cachedProfile;
    private const long THRESHOLD_MB = 2048;

    /// <summary>
    /// Trả về profile đã cache (nếu có). Dùng trong MapViewModel để check nhanh.
    /// </summary>
    public DeviceProfile? CurrentProfile => _cachedProfile;

    /// <summary>
    /// Xác định cấu hình thiết bị dựa trên RAM khả dụng.
    /// Kết quả được cache lại trong vòng đời app.
    /// </summary>
    public Task<DeviceProfile> DetermineProfileAsync()
    {
        if (_cachedProfile.HasValue) return Task.FromResult(_cachedProfile.Value);

        long availableRamMb = GetAvailableMemoryMb();
        _cachedProfile = availableRamMb >= THRESHOLD_MB ? DeviceProfile.HighPerformance : DeviceProfile.PowerSaving;
        return Task.FromResult(_cachedProfile.Value);
    }

    public string GetDisplayName() => _cachedProfile switch
    {
        DeviceProfile.HighPerformance => "Mạnh",
        DeviceProfile.PowerSaving => "Yếu",
        _ => "Chưa xác định"
    };

    private long GetAvailableMemoryMb()
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var activityManager = (ActivityManager?)context.GetSystemService(Context.ActivityService);
        var memoryInfo = new ActivityManager.MemoryInfo();
        if (activityManager != null)
        {
            activityManager.GetMemoryInfo(memoryInfo);
            return memoryInfo.AvailMem / 1048576;
        }
        return THRESHOLD_MB; // Fallback nếu không lấy được ActivityManager
#else
        // Chỉ đo RAM khả dụng trên Android.
        // Các nền tảng ngoài Android (iOS, Windows, MacCatalyst) không cung cấp API lấy RAM rảnh hợp lý,
        // mặc định trả về THRESHOLD_MB để luôn phân loại là HighPerformance.
        return THRESHOLD_MB;
#endif
    }
}
