# AudioTravelling

Ứng dụng thuyết minh tự động bằng GPS tại phố ẩm thực Vĩnh Khánh (Quận 4, TP.HCM). Khách du lịch quét QR → thanh toán VNPay → truy cập PWA 24h → đi vào vùng POI thì audio tự phát theo ngôn ngữ điện thoại. Admin/Owner quản lý qua WebApp riêng.

---

## Cấu trúc thư mục

```
AudioTravelling/
├── DeepTranslateService/   Python FastAPI — dịch text sang đa ngôn ngữ (port 8000)
├── TTSService/             Python FastAPI + edge-tts — text → MP3 (port 8001)
├── ApiService/             C# ASP.NET Core 8 — backend chính (port 5000)
│   ├── AudioTravelling.API/            controllers, hubs, Program.cs
│   ├── AudioTravelling.Core/           entities, interfaces, enums
│   └── AudioTravelling.Infrastructure/ DbContext, LocalizationService, VnPayService
├── mobile-app/             Next.js 14 PWA — app du lịch (port 3000)
├── web-app/                Next.js 14 — admin/owner dashboard (port 3001)
├── docker-compose.yml
├── .env.example            copy → .env rồi điền thông tin thật
└── .gitignore
```

---

## Tech Stack

| Layer | Công nghệ |
|-------|-----------|
| Mobile PWA | Next.js 14 + Workbox + Dexie.js + Leaflet |
| Admin/Owner WebApp | Next.js 14 + Recharts + SignalR client |
| API Backend | C# ASP.NET Core 8 + EF Core 10 |
| Database server | PostgreSQL 16 |
| Cache client | IndexedDB via Dexie.js (trong trình duyệt PWA) |
| DeepTranslate | Python FastAPI + deep-translator |
| TTS | Python FastAPI + edge-tts (miễn phí) |
| Bản đồ | Leaflet.js + OpenStreetMap |
| Thanh toán | VNPay (sandbox: sandbox.vnpayment.vn) |
| Real-time | SignalR Hub tại `/hubs/admin` |

**Không dùng Redis** — session validation qua PostgreSQL (indexed), online count qua SignalR in-memory.

---

## Khởi chạy

```bash
# 1. Tạo file .env
cp .env.example .env
# Điền: POSTGRES_PASSWORD, JWT_SECRET (≥32 ký tự), VNPAY_TMN_CODE, VNPAY_HASH_SECRET

# 2. Chạy tất cả services
docker compose up

# API tự migrate PostgreSQL khi khởi động
```

Sau khi lên, tạo user Admin đầu tiên bằng SQL:
```sql
INSERT INTO "Roles" ("Id","Name") VALUES (1,'Admin'),(2,'Owner') ON CONFLICT DO NOTHING;
INSERT INTO "Users" ("Id","Email","PasswordHash","RoleId","CreatedAt")
VALUES (gen_random_uuid(), 'admin@example.com', '<bcrypt_hash>', 1, now());
```

---

## Database Architecture

```
[SERVER — PostgreSQL 16]          [CLIENT — IndexedDB / Dexie.js]
Nguồn dữ liệu gốc                Cache local, hoạt động offline
Truy cập qua API C#               Truy cập trực tiếp từ JS PWA
```

### PostgreSQL tables

| Table | Mục đích |
|-------|----------|
| `Users` | Admin + Owner (JWT auth) |
| `Roles` | Admin / Owner |
| `Packages` | Basic(15m,P0) / Advanced(30m,P1) / Professional(50m,P2) |
| `Pois` | POI với status: Draft→Pending→Approved/Rejected |
| `PoiImages` | Ảnh của POI |
| `PoiLocalizations` | Text + AudioUrl theo từng ngôn ngữ |
| `PoiApprovalLogs` | Lịch sử duyệt/từ chối |
| `AccessCodes` | QR codes do Admin tạo |
| `AccessSessions` | Session 24h sau khi thanh toán (index trên SessionToken) |

### IndexedDB stores (client, `mobile-app/src/lib/db.ts`)
`CachedPois` · `CachedPoiImages` · `CachedPoiLocalizations` · `CachedPoiAudios` · `PoiGeofenceState` · `AudioPlaybackHistory`

---

## API Endpoints (ApiService)

### Auth
- `POST /api/auth/login` — JWT cho Admin/Owner
- `POST /api/access/verify` — kiểm tra session token tourist

### QR & Access (Admin)
- `POST /api/qr` — tạo QR
- `PATCH /api/qr/{id}/toggle` — bật/tắt
- `DELETE /api/qr/{id}`
- `POST /api/access/pay` — tạo VNPay payment URL từ AccessCode
- `GET /api/access/callback` — VNPay webhook → tạo AccessSession → redirect về PWA với token
- `GET /api/access/bootstrap` — tải toàn bộ POI+audio (header: `X-Session-Token`)

### POI
- `GET/POST /api/pois` — Owner CRUD
- `PATCH /api/pois/{id}/submit` — submit để duyệt
- `GET /api/pois/pending` — danh sách chờ duyệt (Admin)
- `PATCH /api/pois/{id}/approve` — duyệt → tự động trigger localization pipeline
- `PATCH /api/pois/{id}/reject`
- `GET /api/pois/active` — POI đã approved (Tourist, cần X-Session-Token)

### Localization / Audio
- `POST /api/pois/{id}/localize` — trigger DeepTranslate + TTS cho 4 ngôn ngữ mặc định (en/zh/ja/ru)
- `GET /api/pois/{id}/audio?lang=XX` — trả audioUrl, fallback về en

### Packages
- `GET /api/packages`
- `POST/PUT /api/packages` (Admin)

### Stats (Admin)
- `GET /api/stats/sessions?period=day|3days|week|month|year`
- `GET /api/stats/heatmap?period=...`
- `GET /api/stats/realtime`
- SignalR Hub `/hubs/admin` — event `OnlineCount`

---

## Localization Pipeline

Khi POI được approve → `LocalizationService.LocalizePoiAsync()` chạy background:
1. Lấy text tiếng Việt từ `PoiLocalizations`
2. Gọi DeepTranslateService (`POST /translate`) cho en/zh/ja/ru
3. Gọi TTSService (`POST /tts/generate`) → lưu MP3 vào `/storage/audio/{poiId}/{lang}.mp3`
4. Lưu AudioUrl vào `PoiLocalizations`

File audio được serve tĩnh tại `/audio/{poiId}/{lang}.mp3` qua ApiService.

---

## Geofence Engine (`mobile-app/src/workers/geofence.worker.ts`)

Chạy trong **Web Worker**, poll GPS mỗi 5 giây:

1. **Accuracy filter** — bỏ qua nếu accuracy > 50m
2. **Hysteresis** — enter threshold = R−1m, exit threshold = R+1m
3. **Debounce 3s** — phải ổn định trong 3s mới tính là "vào vùng"
4. **Short cooldown 45s** — chống spam ở vùng biên
5. **Long cooldown 15 phút** — tránh nghe lại cùng POI quá nhanh
6. **Overlap** — nhiều POI cùng lúc → sort Priority desc, distance asc, phát POI thắng
7. **Trigger** → postMessage về main thread → phát audio

DeepTranslate flow (trong PWA):
- Tầng 1: Lấy từ IndexedDB (đã cache lúc bootstrap)
- Tầng 2: Online + ngôn ngữ lạ → gọi server → cache lại IndexedDB
- Tầng 3: Offline + ngôn ngữ lạ → fallback tiếng Anh

---

## WebApp Routes

**Owner** (sau login với role Owner):
- `/pois` — danh sách POI, tạo mới, submit duyệt

**Admin** (sau login với role Admin):
- `/admin/pois` — duyệt/từ chối POI pending
- `/admin/qr` — tạo/bật/tắt/xóa QR codes
- `/admin/stats` — biểu đồ truy cập (Recharts)
- `/admin/realtime` — số người online qua SignalR

---

## Lưu ý quan trọng

- **Audio storage**: volume Docker `audio_storage` mount vào cả `tts` lẫn `api` tại `/storage/audio`
- **Session token**: gửi qua header `X-Session-Token` (không phải JWT)
- **JWT**: chỉ dùng cho Admin/Owner, gửi qua `Authorization: Bearer <token>`
- **Ngôn ngữ gốc**: POI lưu bằng tiếng Việt, 4 ngôn ngữ mặc định tự động dịch khi approve
- **EF Migrations**: chạy tự động khi API khởi động (`Database.Migrate()`)
- **Packages seed**: tự động seed 3 packages vào DB qua `OnModelCreating`



Role	Email	Password
Admin	admin@audiotravelling.com	Admin@123
Owner	owner@audiotravelling.com	Owner@123


Flow hoàn chỉnh:


Người dùng vào vùng POI
  ├── vi / en  → phát /audio/{poiId}/{lang}.mp3 từ server (có sẵn)
  │
  └── zh / ja / ru / ... (ngôn ngữ khác)
        ├── Đã có trong IndexedDB (CachedPoiAudio)
        │     → phát từ blob URL
        │
        ├── Chưa có + online
        │     → lấy text từ CachedPoiLocalization hoặc gọi DeepTranslate
        │     → POST /api/tts/proxy → TTS stream (không lưu server)
        │     → cache Blob vào IndexedDB
        │     → phát từ blob URL
        │
        └── Chưa có + offline
              → fallback phát tiếng Anh


Mở /map
  ├── Có cache → hiện map ngay (không chờ mạng)
  │
  ├── Offline → dùng cache, không đồng bộ
  │
  └── Online → gọi /api/access/bootstrap
        ├── Xóa POI đã bị xóa trên server khỏi IndexedDB
        ├── Cập nhật POI thay đổi (tên, tọa độ, mô tả, audioUrl)
        └── Thêm POI mới → map tự cập nhật