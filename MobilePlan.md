# Kế hoạch: Kết nối Mobile (MAUI) với API

## Context

Mobile app (MAUI Android) gọi 4 endpoint chưa tồn tại trên API:
`/api/access/pay`, `/api/access/verify`, `/api/access/bootstrap`, `/api/tts/proxy`.
CORS chỉ cho phép WebAdmin (port 5074), chặn mobile. Không có static file serving cho audio.
Map không có click handler → người dùng không thể tap vào marker để xem POI detail.
Sau khi Admin approve POI, pipeline localize/TTS chưa được kích hoạt.

---

## Thay đổi API

### 1. `Api/appsettings.json`
Thêm:
```json
"TtsServiceUrl": "http://localhost:8000"
```

### 2. `Api/Program.cs` — 4 thay đổi

**a. Thay CORS** — đổi `WithOrigins("http://localhost:5074")` → `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`, giữ nguyên tên policy `"AllowWebAdmin"`.

**b. Thêm HttpClientFactory + đăng ký service** sau `AddScoped<LocalizeService>()`:
```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<LocalizationPipeline>();
```

**c. Sau `var app = builder.Build()`** — tạo thư mục audio + bật static files:
```csharp
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "audio"));
app.UseStaticFiles(); // đặt trước app.UseCors(...)
```

### 3. `Api/Services/LocalizationPipeline.cs` — NEW

Inject: `AppDbContext`, `LocalizeService`, `IHttpClientFactory`, `IConfiguration`, `IWebHostEnvironment`.

Method `LocalizePoiAsync(int poiId)`:
1. Lấy `DescriptionVi` từ `Poi` và `PoiName`
2. Voice map:
   ```
   vi → vi-VN-NamMinhNeural
   en → en-US-ChristopherNeural
   zh → zh-CN-YunjianNeural
   ja → ja-JP-KeitaNeural
   ru → ru-RU-DmitryNeural
   ```
3. Với mỗi lang trong `["vi","en","zh","ja","ru"]`:
   - Dịch text (bỏ qua nếu lang == "vi")
   - `POST {TtsServiceUrl}/tts` với `{text, voice, lang}` → nhận `byte[]`
   - Lưu vào `wwwroot/audio/{poiId}/{lang}.mp3`
   - `FirstOrDefaultAsync` theo `(PoiID, LanguageCode)` → update hoặc insert `PoiLocalization`
   - `AudioUrl = /audio/{poiId}/{lang}.mp3`, `Name = poiName`, `Description = translatedText`
4. `SaveChangesAsync()` 1 lần sau vòng lặp
5. Mỗi lang trong try/catch riêng — lỗi 1 lang không dừng lang khác

### 4. `Api/Controllers/AdminPoisController.cs` — Cập nhật

Constructor: inject thêm `IServiceScopeFactory _scopeFactory`.

Trong `Approve()`, sau `await _context.SaveChangesAsync()`:
```csharp
var capturedId = id;
_ = Task.Run(async () =>
{
    await using var scope = _scopeFactory.CreateAsyncScope();
    var pipeline = scope.ServiceProvider.GetRequiredService<LocalizationPipeline>();
    await pipeline.LocalizePoiAsync(capturedId);
});
```
> **Lý do dùng scope mới**: controller's `AppDbContext` bị dispose khi HTTP response trả về — background task cần `AppDbContext` độc lập.

### 5. `Api/Controllers/AccessController.cs` — NEW

Route: `[Route("api/access")]`, `[ApiController]`, **không có** `[Authorize]`. Inject: `AppDbContext`.

#### POST /api/access/pay — body: `{qrCode, deviceId}`
- Tìm QR: `QrCodeValue == qrCode && IsActive == true`
- Nếu không tìm thấy hoặc đã hết hạn → `400 BadRequest`
- Trả về:
  ```json
  { "paymentUrl": "{scheme}://{host}/api/access/callback?qrCode={qrCode}&deviceId={deviceId}" }
  ```

#### GET /api/access/callback — `?qrCode={}&deviceId={}`
- Tìm QR active
- Tạo `AccessSession`: `QrCodeID, DeviceID, IssuedAt=Now, ExpiredAt=Now+24h, IsRevoked=false`
- `SaveChangesAsync()`
- `Redirect($"audiotravelling://session?sessionId={session.SessionID}&deviceId={deviceId}")`

#### POST /api/access/verify — body: `{sessionId, deviceId}`
- `FindAsync(sessionId)` → kiểm tra DeviceID match + IsRevoked + ExpiredAt
- Trả về: `{ valid: bool, expiredAt: DateTime }`

#### GET /api/access/bootstrap — headers: `X-Session-Id`, `X-Device-Id`
- Parse `X-Session-Id` thành `int`, nếu fail → `401`
- Validate session (DeviceID + IsRevoked + ExpiredAt) → `401` nếu không hợp lệ
- Query: `Pois.Include(Package).Include(Localizations).Where(Status=="Approved" && IsActive)`
- Response:
  ```json
  {
    "pois": [{
      "poiID", "poiName", "descriptionVi", "latitude", "longitude",
      "radius": Package.Radius,
      "priority": Package.Priority,
      "isActive", "coverImageUrl": "", "updatedDate"
    }],
    "localizations": [{
      "localizationID", "poiID", "languageCode",
      "name", "description", "audioUrl", "updatedDate"
    }]
  }
  ```
  > `Poi` entity không có `CoverImageUrl` → luôn trả `""` (mobile không crash).

### 6. `Api/Controllers/TtsProxyController.cs` — NEW

Route: `[Route("api/tts")]`, `[ApiController]`, không auth.  
Inject: `AppDbContext`, `LocalizeService`, `IHttpClientFactory`, `IConfiguration`.

#### POST /api/tts/proxy — body: `{poiId, langCode}`
1. Lấy `PoiLocalization` với `LanguageCode=="vi"`, hoặc fallback lấy `Poi.DescriptionVi`
2. Nếu `langCode != "vi"` → gọi `LocalizeService.TranslateAsync()` → lấy `TranslatedText`
3. Lookup voice từ voice map, nếu lang không hỗ trợ → `400`
4. `POST {TtsServiceUrl}/tts` với `{text, voice, lang}` → nhận `byte[]`
5. Trả về `File(bytes, "audio/mpeg")`
6. Mọi exception → `500`

---

## Thay đổi Mobile

### 7. `Mobile/Views/MapPage.xaml.cs` — Thêm tap handler

Trong `OnAppearing()` (sau các dòng hiện tại):
```csharp
MapControl.Info += OnMapInfo;
```

Override `OnDisappearing()`:
```csharp
protected override void OnDisappearing()
{
    base.OnDisappearing();
    MapControl.Info -= OnMapInfo;
}
```

Thêm method:
```csharp
private void OnMapInfo(object? sender, MapInfoEventArgs e)
{
    if (e.MapInfo?.Feature is not PointFeature feature) return;
    if (feature["poi"] is not CachedPoi poi) return;
    MainThread.BeginInvokeOnMainThread(async () =>
        await _vm.NavigateToDetailCommand.ExecuteAsync(poi));
}
```
Using cần thêm: `Mapsui.UI.Maui` (cho `MapInfoEventArgs`).

### 8. `Mobile/Services/ApiService.cs` — Thêm TTS proxy method

```csharp
public async Task<Stream?> TtsProxyAsync(int poiId, string langCode)
{
    try
    {
        var res = await _http.PostAsJsonAsync("/api/tts/proxy", new { poiId, langCode });
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadAsStreamAsync();
    }
    catch { return null; }
}
```

### 9. `Mobile/Services/AudioService.cs` — Cấu trúc lại `ResolveAudioPathAsync()`

Thay thế toàn bộ method `ResolveAudioPathAsync` bằng:

```csharp
private async Task<string?> ResolveAudioPathAsync(int poiId, string langCode)
{
    // 1. Local cache
    var cached = await _db.GetAudioAsync(poiId, langCode);
    if (cached is not null && !string.IsNullOrEmpty(cached.LocalFilePath)
        && File.Exists(cached.LocalFilePath))
        return cached.LocalFilePath;

    // 2. Download từ AudioUrl có sẵn trong localization
    string? downloadedPath = null;
    var localization = await _db.GetLocalizationAsync(poiId, langCode);
    if (localization is not null && !string.IsNullOrEmpty(localization.AudioUrl))
        downloadedPath = await _api.DownloadAudioAsync(localization.AudioUrl, poiId, langCode);

    // 3. TTS proxy fallback (ngôn ngữ không có sẵn hoặc download thất bại)
    if (downloadedPath is null)
    {
        var stream = await _api.TtsProxyAsync(poiId, langCode);
        if (stream is not null)
        {
            var dir = Path.Combine(FileSystem.AppDataDirectory, "audio", poiId.ToString());
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{langCode}.mp3");
            await using var fs = File.Create(path);
            await stream.CopyToAsync(fs);
            downloadedPath = path;
        }
    }

    if (downloadedPath is null) return null;

    // 4. Lưu vào SQLite cache (dùng lại record cũ để tránh duplicate insert)
    var record = cached ?? new CachedPoiAudio { PoiID = poiId, LanguageCode = langCode };
    record.AudioUrl = localization?.AudioUrl ?? string.Empty;
    record.LocalFilePath = downloadedPath;
    record.DownloadStatus = "downloaded";
    record.CachedAt = DateTime.UtcNow;
    await _db.SaveAudioAsync(record);

    return downloadedPath;
}
```

---

## Logic xử lý 2 POI lồng lên nhau

**Đã implement đúng trong `GeofenceService.cs` — KHÔNG cần thay đổi.**

Luồng hiện tại:
1. Tick mỗi 5s, tính khoảng cách Haversine đến tất cả POI
2. Mỗi POI có state machine riêng (debounce 3s, hysteresis ±1m)
3. Thu thập `candidates` = tất cả POI người dùng vừa enter VÀ cả 2 cooldown đã hết
4. **Chọn winner**: `OrderByDescending(Priority).ThenBy(distance).First()`
5. Chỉ winner bị set cooldown (45s + 15 phút) — POI khác vẫn eligible
6. Winner → phát audio

**Kịch bản 2 POI chồng lên nhau:**
- POI A (Priority=2) + POI B (Priority=1), người dùng đứng trong cả 2 → A thắng
- 45s sau A hết short cooldown → B thắng lần tiếp (hoặc A nếu long cooldown cũng hết)
- Cùng priority → POI gần hơn thắng

---

## Thứ tự thực hiện

| # | File | Loại |
|---|------|------|
| 1 | `Api/appsettings.json` | Cập nhật |
| 2 | `Api/Program.cs` | Cập nhật |
| 3 | `Api/Services/LocalizationPipeline.cs` | Tạo mới |
| 4 | `Api/Controllers/AdminPoisController.cs` | Cập nhật |
| 5 | `Api/Controllers/AccessController.cs` | Tạo mới |
| 6 | `Api/Controllers/TtsProxyController.cs` | Tạo mới |
| 7 | `Mobile/Views/MapPage.xaml.cs` | Cập nhật |
| 8 | `Mobile/Services/ApiService.cs` | Cập nhật |
| 9 | `Mobile/Services/AudioService.cs` | Cập nhật |

---

## Kiểm tra end-to-end

1. Khởi động: SQL Server + TTSService (`uvicorn app:app --port 8000`) + API (`dotnet run`)
2. Test CORS: `OPTIONS /api/access/bootstrap` với `Origin: http://10.0.2.2` → `Access-Control-Allow-Origin: *`
3. Test static files: tạo file dummy ở `wwwroot/audio/test.mp3` → `GET /audio/test.mp3` trả về file
4. Admin tạo QR qua Swagger → ghi nhớ `code`
5. Test `/api/access/pay` với code → nhận `paymentUrl`
6. Mở `paymentUrl` trong browser emulator → deep link fire → app navigate tới map
7. Test `/api/access/verify` với sessionId vừa tạo → `{valid: true}`
8. Admin approve một POI → đợi ~10s → kiểm tra `wwwroot/audio/{id}/vi.mp3` tồn tại
9. Test `/api/access/bootstrap` với session headers → nhận pois + localizations
10. Trong app: tap vào marker trên bản đồ → PoiDetailPage mở ra
11. Tap "Phát audio" → audio phát (từ server hoặc TTS proxy)
