VNPay flow chi tiết
App gọi POST /api/access/pay với { qrCode: "xxx", deviceId: "yyy" }
API tìm QrCodes row theo QrCode = qrCode → validate IsActive = 1, ExpiredAt > NOW()
Tạo VNPay payment URL (HMAC-SHA512):
vnp_TmnCode — từ appsettings.json
vnp_HashSecret — từ appsettings.json
vnp_Amount — từ appsettings.json["AccessPrice"] (VD: 5000000 = 50,000 VND)
vnp_ReturnUrl — https://{host}/api/access/callback
vnp_OrderInfo — "AudioTravelling 24h - {deviceId}"
vnp_TxnRef — {qrCodeId}_{deviceId}_{timestamp}
Trả về { paymentUrl }
VNPay callback → API validate HMAC → vnp_ResponseCode == "00" → tạo AccessSession:
QrCodeID, DeviceID, IssuedAt = NOW(), ExpiredAt = NOW() + 24h
SessionToken = Guid.NewGuid().ToString("N")
Redirect 301 → audiotravelling://session?token={SessionToken}


App launch
  └── SplashPage (check SQLite for valid session)
        ├── Session hợp lệ → MapPage (bootstrap if online)
        └── Session không có/hết hạn → QrScanPage
              └── Scan QR → gọi /api/access/pay → Browser.OpenAsync(paymentUrl)
                    └── VNPay thanh toán → deep link audiotravelling://session?token=X
                          └── MainActivity.OnNewIntent → extract token → lưu SQLite
                                └── Bootstrap API → MapPage

Splash → vào Map ngay (từ cache) → Map hiện nhanh
MapPage.OnAppearing:
  ├── Load POIs từ SQLite cache → hiện lên map NGAY
  └── Background task:
      ├── Kiểm tra connectivity
      ├── Nếu online → gọi bootstrap API
      ├── Upsert POIs mới vào SQLite
      └── RefreshLayers() → map tự cập nhật markers
Cách này:

POI mới xuất hiện sau vài giây ở background mà không block UI
Offline vẫn hoạt động bình thường
Map hiện ngay lập tức từ cache (không chờ)

Splash:
  ├── verify session → if valid → GoToAsync("//map") NGAY (không sync)
  └── if error      → GoToAsync("//map") hoặc "//qrscan"

MapPage.OnAppearing():
  ├── (1) LoadPoisCommand → SQLite cache → render map NGAY
  ├── (2) _ = SyncAndRefreshAsync()  ← fire-and-forget
  │         ├── check Connectivity.NetworkAccess == Internet
  │         ├── offline → return (bỏ qua)
  │         ├── _vm.SyncFromApiAsync()
  │         │     ├── _api.BootstrapAsync()
  │         │     ├── _db.UpsertPoisAsync(pois)
  │         │     ├── _db.UpsertLocalizationsAsync(locs)
  │         │     └── returns true/false
  │         └── if synced → RefreshLayersAsync() → map cập nhật markers
  └── (3) StartLocationPolling()

  Mở app → SplashPage → session SQLite còn hạn
  → GoToAsync("//map") [không cần QR]
  → MapPage.OnAppearing()
      → SyncAndRefreshAsync() [nếu có WiFi]
          → BootstrapAsync() → cập nhật POI + localizations
          → LocalizationPreloader.PreloadAsync(poiIds, "ja")
                Mỗi POI:
                  ├── Audio file đã có? → skip
                  ├── Có AudioUrl? → tải file về → lưu CachedPoiAudio
                  └── Chưa có gì? → TTS proxy → lưu localization + lưu audio file
Trường hợp 2 — Admin xóa session, user quét lại QR

Mở app → SplashPage → session invalid → GoToAsync("//qrscan")
  → Dev-bypass → Bootstrap → preload ngôn ngữ điện thoại
  → GoToAsync("//map") → MapPage → sync lại (idempotent, skip đã có)
Kết quả khi vào geofence zone

GeofenceService trigger → AudioService.PlayAsync(poi, "ja")
  → ResolveAudioPathAsync("ja")
      → CachedPoiAudio tồn tại + file trên disk? → phát ngay ✓ (không tải gì thêm)