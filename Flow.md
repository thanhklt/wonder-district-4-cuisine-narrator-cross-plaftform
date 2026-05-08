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