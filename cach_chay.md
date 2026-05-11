# Cách chạy hệ thống AudioTravelling

Hướng dẫn này dùng cho PowerShell tại thư mục gốc dự án:

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform
```

Nên mở mỗi service ở một cửa sổ PowerShell riêng để dễ theo dõi log.

## 1. Chạy TTSService

TTSService chạy ở port `8000`. API backend đang gọi TTS qua cấu hình `TtsServiceUrl`.

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform\TTSService
.\.venv\Scripts\Activate.ps1
uvicorn app:app --host 0.0.0.0 --port 8000 --reload
```

Nếu chưa có thư viện Python:

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform\TTSService
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
uvicorn app:app --host 0.0.0.0 --port 8000 --reload
```

## 2. Chạy API backend

API chạy ở port `5184`. Dùng profile `http` để API lắng nghe trên `0.0.0.0:5184`, phù hợp cho mobile/ngrok.

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform
dotnet run --project Api\Api.csproj --launch-profile http
```

Kiểm tra API còn sống:

```powershell
Invoke-WebRequest http://localhost:5184/api/packages -UseBasicParsing
```

Endpoint tạo phiên dev cho mobile:

```powershell
Invoke-WebRequest `
  -Uri http://localhost:5184/api/access/dev-bypass `
  -Method Post `
  -ContentType 'application/json' `
  -Body '{"deviceId":"test-device"}' `
  -UseBasicParsing
```

## 3. Chạy ngrok cho API

Ngrok dùng để điện thoại hoặc máy khác truy cập API mà không phụ thuộc Windows Firewall/LAN.

Nếu lần đầu dùng ngrok, đăng nhập và thêm authtoken:

```powershell
ngrok config add-authtoken YOUR_TOKEN_HERE
```

Không gửi token lên chat hoặc commit vào git.

Sau khi API đã chạy ở port `5184`, mở cửa sổ PowerShell mới:

```powershell
ngrok http 5184
```

Ngrok sẽ hiện dòng dạng:

```text
Forwarding https://xxxx.ngrok-free.app -> http://localhost:5184
```

URL cần dùng cho mobile/WebAdmin là:

```text
https://xxxx.ngrok-free.app
```

Với mobile, URL API nằm trong:

```text
Mobile\Services\ApiService.cs
```

Ví dụ:

```csharp
private const string BaseUrl = "https://xxxx.ngrok-free.app";
```

Với WebAdmin, URL API nằm trong 2 file:

```text
WebAdmin\wwwroot\js\core\apiClient.js
WebAdmin\wwwroot\js\core\auth.js
```

Giá trị cần dùng có thêm `/api`:

```js
var BASE_URL = 'https://xxxx.ngrok-free.app/api';
var API_BASE_URL = 'https://xxxx.ngrok-free.app/api';
```

Sau khi đổi URL, cần build/cài lại mobile APK. WebAdmin thì refresh trình duyệt bằng `Ctrl + F5`.

## 4. Build APK mobile

Chạy từ thư mục gốc dự án:

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform
dotnet clean Mobile\Mobile.csproj -f net10.0-android -c Debug
dotnet build Mobile\Mobile.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
```

File APK sau khi build:

```text
Mobile\bin\Debug\net10.0-android\com.audiotravelling.mobile-Signed.apk
```

## 5. Cài APK vào điện thoại bằng ADB

Cắm điện thoại qua USB, bật Developer Options và USB debugging.

```powershell
$adb = 'C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe'
$apk = 'D:\wonder-district-4-cuisine-narrator-cross-plaftform\Mobile\bin\Debug\net10.0-android\com.audiotravelling.mobile-Signed.apk'

& $adb devices
& $adb install -r $apk
```

Nếu báo `device unauthorized`, mở khóa điện thoại và bấm Allow ở popup USB debugging, rồi chạy lại:

```powershell
& $adb kill-server
& $adb start-server
& $adb devices
& $adb install -r $apk
```

Nếu báo `signatures do not match`, gỡ app cũ trên điện thoại rồi cài lại:

```powershell
& $adb uninstall com.audiotravelling.mobile
& $adb install -r $apk
```

## 6. Tùy chọn: dùng ADB reverse thay cho ngrok

Nếu chỉ test khi điện thoại đang cắm USB, có thể dùng:

```powershell
$adb = 'C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe'
& $adb reverse tcp:5184 tcp:5184
```

Khi dùng cách này, mobile phải gọi API qua:

```text
http://127.0.0.1:5184
```

Ngrok tiện hơn nếu muốn cầm điện thoại dùng không cần cáp.

## 7. Chạy WebAdmin

WebAdmin chạy ở port `5074`.

```powershell
cd D:\wonder-district-4-cuisine-narrator-cross-plaftform
dotnet run --project WebAdmin\WebAdmin.csproj --launch-profile http
```

Mở trình duyệt:

```text
http://localhost:5074
```

Nếu WebAdmin gọi API qua ngrok, nhớ đổi 2 file JS đã nêu ở bước 3 sang:

```text
https://xxxx.ngrok-free.app/api
```

## Thứ tự chạy khuyến nghị

1. Chạy `TTSService` ở port `8000`.
2. Chạy `Api` ở port `5184`.
3. Chạy `ngrok http 5184`.
4. Cập nhật URL ngrok cho mobile/WebAdmin nếu cần.
5. Build APK và cài bằng `adb install -r`.
6. Chạy `WebAdmin` ở port `5074`.

## Lỗi thường gặp

### Mobile báo không thể kết nối API

Kiểm tra:

```powershell
Invoke-WebRequest http://localhost:5184/api/access/dev-bypass -Method Post -ContentType 'application/json' -Body '{"deviceId":"test"}' -UseBasicParsing
```

Nếu dùng ngrok, kiểm tra URL mới nhất trong cửa sổ ngrok và cập nhật lại mobile/WebAdmin.

### Ngrok báo cần authtoken

Chạy:

```powershell
ngrok config add-authtoken YOUR_TOKEN_HERE
```

### Điện thoại cùng WiFi nhưng không gọi được IP máy tính

Có thể Windows Firewall chặn port `5184`. Mở PowerShell bằng Run as Administrator:

```powershell
netsh advfirewall firewall add rule name="AudioTravelling API 5184" dir=in action=allow protocol=TCP localport=5184
```

Hoặc dùng ngrok để tránh phụ thuộc LAN/firewall.
