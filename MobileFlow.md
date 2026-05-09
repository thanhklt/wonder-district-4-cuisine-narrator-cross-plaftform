Đây là ứng dụng .NET MAUI — tức là một app di động native (Android/iOS) chạy song song với PWA Next.js đã được mô tả trong CLAUDE.md. Dưới đây là giải thích toàn bộ:

Kiến trúc tổng thể

Mobile/
├── MauiProgram.cs          ← Điểm khởi động, đăng ký DI
├── App.xaml.cs             ← App shell
├── Models/                 ← Dữ liệu SQLite local
├── Services/               ← Logic nghiệp vụ
├── ViewModels/             ← MVVM: xử lý state & logic UI
├── Views/                  ← XAML: giao diện người dùng
└── Platforms/Android/      ← Code Android-specific (foreground service)
Pattern: MVVM (Model-View-ViewModel) với CommunityToolkit.Mvvm.

Luồng điều hướng

Mở app
  └── SplashPage (kiểm tra session)
        ├── Có session hợp lệ → MapPage (bản đồ chính)
        └── Không có → QrScanPage (quét QR hoặc dev bypass)
                          └── Thanh toán VNPay xong → Bootstrap POI → MapPage
Các Services (Singleton)
Service	Nhiệm vụ
DatabaseService.cs	SQLite local — lưu POI, audio, session
ApiService.cs	Gọi API backend (bootstrap, verify, TTS proxy)
AudioService.cs	Phát audio với fallback: cache → tải về → TTS proxy
SessionService.cs	Device ID (SHA256), lưu/đọc session token
GeofenceService.cs	Poll GPS 5s, tính khoảng cách Haversine, trigger khi vào vùng POI
3 file đã được sửa gần đây
MapViewModel.cs
Load danh sách POI từ SQLite → hiển thị lên bản đồ Mapsui
Lắng nghe GeofenceTriggeredMessage (WeakReferenceMessenger) khi vào vùng POI
Điều khiển popup thông tin POI và nút play/stop audio
SplashViewModel.cs
Khởi động app: gọi ApiService.VerifySessionAsync()
Điều hướng đến MapPage hoặc QrScanPage tùy kết quả
MapPage.xaml.cs
Code-behind của bản đồ: khởi tạo 3 layer Mapsui
Layer 1: Marker cam cho từng POI
Layer 2: Vòng tròn đứt nét thể hiện bán kính geofence
Layer 3: Chấm xanh vị trí người dùng
Bottom-sheet popup khi chạm vào POI
Geofence Engine (Android Background Service)
GeofenceForegroundService.cs chạy ngầm:

Poll GPS mỗi 5 giây
Tính khoảng cách Haversine đến từng POI
Debounce 3s — phải ổn định mới tính là "vào vùng"
Cooldown 45s ngắn + 15 phút dài để chống phát lại
Nhiều POI chồng nhau → ưu tiên theo Priority rồi distance
Gửi GeofenceTriggeredMessage → MapViewModel nhận → phát audio
Chuỗi phát audio

Vào vùng POI
  └── AudioService.PlayForPoiAsync()
        ├── Có trong SQLite cache → phát blob
        ├── Có AudioUrl → tải MP3 → cache → phát
        └── Không có (ngôn ngữ lạ) → POST /api/tts/proxy → stream → cache → phát