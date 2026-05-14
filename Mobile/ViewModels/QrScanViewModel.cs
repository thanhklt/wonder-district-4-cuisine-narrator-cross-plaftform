using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class QrScanViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly SessionService _session;
    private readonly DatabaseService _db;
    private readonly DeviceConfigService _deviceConfig;

    public QrScanViewModel(ApiService api, SessionService session, DatabaseService db, DeviceConfigService deviceConfig)
    {
        _api = api;
        _session = session;
        _db = db;
        _deviceConfig = deviceConfig;
    }

    [ObservableProperty]
    private bool _isScannerVisible = true;

    [RelayCommand]
    public async Task OnQrDetectedAsync(string qrCode)
    {
        if (IsBusy) return;
        IsBusy = true;
        IsScannerVisible = false;
        StatusMessage = "Đang xử lý mã QR...";

        try
        {
            // Đo cấu hình TRƯỚC KHI tạo session
            var profile = await _deviceConfig.DetermineProfileAsync();

            var paymentUrl = await _api.GetPaymentUrlAsync(qrCode, (int)profile);
            if (paymentUrl is null)
            {
                StatusMessage = "Mã QR không hợp lệ hoặc đã hết hạn.";
                IsScannerVisible = true;
                return;
            }

            StatusMessage = "Đang mở trang thanh toán...";
            await Browser.OpenAsync(paymentUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            StatusMessage = "Có lỗi xảy ra. Vui lòng thử lại.";
            IsScannerVisible = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void RetryScan()
    {
        StatusMessage = string.Empty;
        IsScannerVisible = true;
    }

    // Dev bypass: tao session THAT trong DB voi device ID thuc, khong can quet QR
    [RelayCommand]
    public async Task SimulatePaymentAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Đang tạo phiên dev...";

        try
        {
            // 1. Đo cấu hình TRƯỚC KHI tạo session
            var profile = await _deviceConfig.DetermineProfileAsync();
            var deviceId = _session.GetDeviceId();

            // 2. Gọi API tạo session với DeviceProfile
            var bypass = await _api.DevBypassAsync(deviceId, (int)profile);
            if (bypass is null)
            {
                StatusMessage = "Không thể kết nối API. Kiểm tra IP/port và server đang chạy.";
                return;
            }

            // Luu session vao local DB
            await _session.SaveSessionAsync(bypass.SessionId, bypass.ExpiredAt);
            StatusMessage = $"Session #{bypass.SessionId} đã được tạo. Đang tải dữ liệu...";

            // 3. Hiện thông báo cấu hình
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Cấu hình thiết bị", _deviceConfig.GetDisplayName(), "OK");

            // 4. Phân nhánh Bootstrap: chỉ chạy nếu HighPerformance
            if (profile == Models.DeviceProfile.HighPerformance)
            {
                var data = await _api.BootstrapAsync();
                if (data is not null)
                {
                    var pois = data.Pois.Select(p => new Models.CachedPoi
                    {
                        PoiID = p.PoiID, PoiName = p.PoiName, DescriptionVi = p.DescriptionVi,
                        Latitude = p.Latitude, Longitude = p.Longitude,
                        Radius = p.Radius, Priority = p.Priority,
                        IsActive = p.IsActive, CoverImageUrl = p.CoverImageUrl,
                        UpdatedDate = p.UpdatedDate, CachedAt = DateTime.UtcNow
                    });
                    await _db.UpsertPoisAsync(pois);

                    var locs = data.Localizations.Select(l => new Models.CachedPoiLocalization
                    {
                        LocalizationID = l.LocalizationID, PoiID = l.PoiID,
                        LanguageCode = l.LanguageCode, Name = l.Name,
                        Description = l.Description, AudioUrl = l.AudioUrl,
                        UpdatedDate = l.UpdatedDate, CachedAt = DateTime.UtcNow
                    });
                    await _db.UpsertLocalizationsAsync(locs);

                    // Preload localization theo ngôn ngữ điện thoại (background, không block)
                    var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    var poiIds = data.Pois.Select(p => p.PoiID).ToList();
                    _ = LocalizationPreloader.PreloadAsync(poiIds, lang, _db, _api);
                }
            }
            // Nếu PowerSaving: bỏ qua Bootstrap, MapViewModel sẽ tự fetch online.

            await Shell.Current.GoToAsync("//map");
        }
        catch
        {
            StatusMessage = "Không thể kết nối API. Kiểm tra IP/port và server đang chạy.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
