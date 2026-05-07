using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class QrScanViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly SessionService _session;
    private readonly DatabaseService _db;

    public QrScanViewModel(ApiService api, SessionService session, DatabaseService db)
    {
        _api = api;
        _session = session;
        _db = db;
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
            var paymentUrl = await _api.GetPaymentUrlAsync(qrCode);
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
            var deviceId = _session.GetDeviceId();

            // Goi API tao session that voi device ID thuc
            var bypass = await _api.DevBypassAsync(deviceId);
            if (bypass is null)
            {
                StatusMessage = "Không thể kết nối API. Kiểm tra IP/port và server đang chạy.";
                return;
            }

            // Luu session vao local DB
            await _session.SaveSessionAsync(bypass.SessionId, bypass.ExpiredAt);
            StatusMessage = $"Session #{bypass.SessionId} đã được tạo. Đang tải dữ liệu...";

            // Bootstrap POI
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
            }

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
