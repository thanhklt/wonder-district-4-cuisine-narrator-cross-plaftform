# Cách khởi chạy dự án

## Lựa chọn 1 — Kết nối qua USB (ADB Reverse) - Ổn định nhất
Dùng lệnh này trong PowerShell để tự động kết nối tất cả thiết bị đang cắm:
```powershell
$adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"; & $adb devices | Select-String "device$" | ForEach-Object { & $adb -s ($_ -split "\s+")[0] reverse tcp:5184 tcp:5184 }
```
*Lưu ý: Trong `ApiService.cs`, để `BaseUrl = "http://localhost:5184"`.*

## Lựa chọn 2 — Kết nối qua WiFi LAN (Cho nhiều máy không cần cáp)
1. Mở PowerShell (Admin) và chạy lệnh mở Firewall:
```powershell
New-NetFirewallRule -DisplayName "Allow API Port 5184" -Direction Inbound -LocalPort 5184 -Protocol TCP -Action Allow
```
2. Đảm bảo điện thoại và máy tính chung WiFi.
3. Trong `ApiService.cs`, đổi `BaseUrl` thành IP máy tính (Ví dụ: `http://10.120.55.47:5184`).

---

## Bước 2 — Khởi động API Backend
```bash
cd Api
dotnet run
```

## Bước 3 — Khởi động TTS Service
```bash
cd TTSService
uvicorn app:app --port 8000 --reload
```

## Bước 4 — Khởi động WebAdmin
```bash
cd WebAdmin
dotnet run
```
