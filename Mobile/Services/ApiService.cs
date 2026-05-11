using System.Net.Http.Json;
using Mobile.Models;

namespace Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly SessionService _session;

    // USB (adb reverse): http://localhost:5184 | Emulator: http://10.0.2.2:5184 | WiFi LAN: http://192.168.1.198:5184
    // private const string BaseUrl = "http://192.168.1.198:5184";

    private const string BaseUrl = "https://scraggly-plausibly-synthesis.ngrok-free.dev"; // Thay thế = ngrok mới mỗi khi chạy

    public ApiService(SessionService session)
    {
        _session = session;
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10) // tranh treo vo han khi API khong phan hoi
        };
    }

    private async Task AddSessionHeadersAsync()
    {
        var s = await _session.GetValidSessionAsync();
        if (s is null) return;
        _http.DefaultRequestHeaders.Remove("X-Session-Id");
        _http.DefaultRequestHeaders.Remove("X-Device-Id");
        _http.DefaultRequestHeaders.Add("X-Session-Id", s.SessionID.ToString());
        _http.DefaultRequestHeaders.Add("X-Device-Id", s.DeviceID);
    }

    // Gọi VNPay → trả về paymentUrl
    public async Task<string?> GetPaymentUrlAsync(string qrCode)
    {
        var deviceId = _session.GetDeviceId();
        var res = await _http.PostAsJsonAsync("/api/access/pay", new { qrCode, deviceId });
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadFromJsonAsync<PayResponseDto>();
        return json?.PaymentUrl;
    }

    // Kiểm tra session còn hợp lệ không
    public async Task<VerifyResponseDto?> VerifySessionAsync(int sessionId, string deviceId)
    {
        var res = await _http.PostAsJsonAsync("/api/access/verify", new { sessionId, deviceId });
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<VerifyResponseDto>();
    }

    // Tải toàn bộ POI + localizations
    public async Task<BootstrapResponseDto?> BootstrapAsync()
    {
        await AddSessionHeadersAsync();
        var res = await _http.GetAsync("/api/access/bootstrap");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<BootstrapResponseDto>();
    }

    // Tao session that trong DB bang dev bypass (khong can quet QR)
    public async Task<DevBypassResponseDto?> DevBypassAsync(string deviceId)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/access/dev-bypass", new { deviceId });
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<DevBypassResponseDto>();
        }
        catch { return null; }
    }

    // Lay danh sach anh cua 1 POI (khi online, dung cho carousel PoiDetailPage)
    public async Task<List<PoiImageRecord>?> GetPoiImagesAsync(int poiId)
    {
        try
        {
            await AddSessionHeadersAsync();
            var res = await _http.GetAsync($"/api/access/pois/{poiId}/images");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<List<PoiImageRecord>>();
        }
        catch { return null; }
    }

    // Goi TTS proxy de tao audio on-demand cho ngon ngu chua co san
    // Tra ve bytes audio + text da dich (de luu SQLite, tranh phai goi lai API)
    public async Task<TtsProxyResult?> TtsProxyAsync(int poiId, string langCode)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/tts/proxy", new { poiId, langCode });
            if (!res.IsSuccessStatusCode) return null;

            var bytes = await res.Content.ReadAsByteArrayAsync();

            string GetHeader(string key)
            {
                return res.Headers.TryGetValues(key, out var vals)
                    ? Uri.UnescapeDataString(vals.First())
                    : string.Empty;
            }

            return new TtsProxyResult(
                AudioBytes:  bytes,
                Description: GetHeader("X-Translated-Description"),
                Name:        GetHeader("X-Translated-Name"),
                AudioUrl:    GetHeader("X-Audio-Url")
            );
        }
        catch { return null; }
    }

    // Tải file audio về local
    public async Task<string?> DownloadAudioAsync(string audioUrl, int poiId, string langCode)
    {
        try
        {
            var res = await _http.GetAsync(audioUrl);
            if (!res.IsSuccessStatusCode) return null;

            var dir = Path.Combine(FileSystem.AppDataDirectory, "audio", poiId.ToString());
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{langCode}.mp3");

            await using var fs = File.Create(path);
            await (await res.Content.ReadAsStreamAsync()).CopyToAsync(fs);
            return path;
        }
        catch { return null; }
    }
}

public record PayResponseDto(string PaymentUrl);
public record VerifyResponseDto(bool Valid, DateTime ExpiredAt);
public record DevBypassResponseDto(int SessionId, DateTime ExpiredAt);
public record BootstrapResponseDto(
    List<PoiDto> Pois,
    List<LocalizationDto> Localizations);
public record PoiDto(int PoiID, string PoiName, string DescriptionVi,
    double Latitude, double Longitude, int Radius, int Priority,
    bool IsActive, string CoverImageUrl, DateTime UpdatedDate);
public record LocalizationDto(int LocalizationID, int PoiID, string LanguageCode,
    string Name, string Description, string AudioUrl, DateTime UpdatedDate);
public record PoiImageRecord(int ImageID, string ImageUrl, bool IsCover, int DisplayOrder);
public record TtsProxyResult(byte[] AudioBytes, string Description, string Name, string AudioUrl);
