# Cách khởi chạy dự án

## Bước 1 — Kết nối điện thoại Android (ADB Reverse)
```bash
adb reverse tcp:5184 tcp:5184
```

## Bước 2 — Khởi động API Backend
```bash
cd Api
dotnet run
```

## Bước 3 — Khởi động TTS Service
```bash
cd TTService
uvicorn main:app --port 8000 --reload
```

## Bước 4 — Khởi động WebAdmin
```bash
cd WebAdmin
dotnet run
```
