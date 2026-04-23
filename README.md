# AudioTravelling — Product Requirements Document

---

## 1. Overview

Hệ thống là một ứng dụng di động hỗ trợ thuyết minh tự động tại phố ẩm thực Vĩnh Khánh (Quận 4), giúp khách du lịch tiếp cận thông tin về các quán ăn thông qua audio theo vị trí GPS.  
Hệ thống sử dụng cơ chế geofence để tự động phát nội dung khi người dùng đi vào vùng của POI.  
Admin và Owner sẽ sử dụng webapp để quản lý.

---

## 2. User Roles

| Role | Mô tả |
|------|-------|
| **User** (khách du lịch) | Quét QR → thanh toán VNPay → truy cập PWA 24h. Không cần đăng nhập tài khoản. |
| **Owner** (chủ quán) | Tạo/sửa/xóa POI, gửi yêu cầu duyệt đến Admin. |
| **Admin** | Quản lý QR, duyệt POI, xem thống kê truy cập theo thời gian thực và lịch sử. |

---

## 3. Scope

- Hệ thống phải quản lý thread và API để nhiều người cùng sử dụng đồng thời.
- Khi user đăng nhập lần đầu sẽ tự động tải toàn bộ tài nguyên cần thiết để dùng offline.
- Mobile app sử dụng **Progressive Web App** (không cần cài từ store).
- Owner/Admin sử dụng **WebApp** riêng.
- Backend: C# ASP.NET Core 8.

### Mobile App
- Theo dõi GPS ở cả foreground và background
- Hiển thị vị trí user và các POI active trên bản đồ
- Tự động phát audio khi vào vùng geofence của POI
- Cho phép user bấm vào POI để xem thông tin và phát audio thủ công
- Hỗ trợ online và offline
- Dùng IndexedDB (Dexie.js) để cache POI, localization, audio

### Geofence & Anti-spam
- Lấy vị trí người dùng mỗi 5 giây
- Tính khoảng cách user–POI
- Trigger theo bán kính R của POI
- Xử lý khi nhiều POI có bán kính trùng nhau (Priority Queue)
- Chống spam: accuracy filter, buffer zone, debounce, short cooldown (45s)

### Audio Thuyết minh
- Mỗi POI có text giới thiệu tiếng Việt
- Tự động detect ngôn ngữ điện thoại
- Phát audio theo ngôn ngữ thiết bị
- Dừng audio cũ khi phát audio mới

### Package
| Gói | Bán kính | Priority |
|-----|----------|----------|
| Cơ bản | 15m | 0 |
| Nâng cao | 30m | 1 |
| Chuyên nghiệp | 50m | 2 |

### Text-to-Speech
- POI gốc lưu tiếng Việt
- Bootstrap chỉ trả về `vi` và `en`
- Các ngôn ngữ khác: generate on-demand qua TTS proxy, lưu server + cache client
- Server lưu audio tại `storage/audio/{poiId}/{language}.mp3`

### Quản lý POI
- Owner tạo POI (Draft) → Submit → Admin duyệt/từ chối
- Chỉ POI Approved mới hiển thị trên map

### Quản lý Truy cập
- Admin xem số người online theo thời gian thực (SignalR)
- Thống kê theo ngày / 3 ngày / tuần / tháng / năm
- Heatmap truy cập

### Quản lý QR
- Admin tạo / bật / tắt / xóa mã QR
- Mã QR nhúng URL → trang thanh toán VNPay

---

## 4. Services

| Service | Công nghệ | Cổng | Mô tả |
|---------|-----------|------|-------|
| **API** | C# ASP.NET Core 8 | 5000 | Backend chính, kết nối DB, xác thực JWT |
| **DeepTranslate** | Python FastAPI + DeepL | 8000 | Dịch text POI sang đa ngôn ngữ |
| **TTS** | Python FastAPI + edge-tts | 8001 | Chuyển text → MP3, lưu file |
| **Mobile** | Next.js 14 PWA | 3000 | Giao diện khách du lịch |
| **WebApp** | Next.js 14 | 3001 | Giao diện Admin/Owner |
| **nginx** | nginx:alpine | 80/443 | Reverse proxy, route `/api/` và `/` |

---

## 5. Database

### Server Database (SQL Server)

| Bảng | Mô tả |
|------|-------|
| `Users` | Admin + Owner |
| `Roles` | Admin / Owner |
| `Packages` | Cơ bản / Nâng cao / Chuyên nghiệp |
| `Pois` | Thông tin POI, trạng thái Draft→Pending→Approved/Rejected |
| `PoiImages` | Ảnh của POI |
| `PoiLocalizations` | Text + AudioUrl theo từng ngôn ngữ |
| `PoiApprovalLogs` | Lịch sử duyệt/từ chối |
| `AccessCodes` | Mã QR do Admin tạo |
| `AccessSessions` | Phiên truy cập 24h sau thanh toán |

### Client Cache (IndexedDB — Dexie.js)

| Store | Mô tả |
|-------|-------|
| `pois` | Metadata POI (id, tọa độ, bán kính, priority) |
| `poiImages` | URL ảnh |
| `poiLocalizations` | Text + audioUrl (chỉ `vi` và `en` từ bootstrap) |
| `poiAudios` | Blob audio cho ngôn ngữ được generate on-demand |
| `geofenceState` | Trạng thái vào/ra zone, cooldown mỗi POI |
| `playbackHistory` | Lịch sử phát audio |

---

## 6. Flow Geofence

### Mô tả các giai đoạn

**Giai đoạn 1 — Nhận GPS**
- App nhận vị trí GPS mỗi 5 giây (main thread)
- Cập nhật vị trí user trên map
- Gửi `{lat, lng, accuracy}` sang Geofence Worker
- Bỏ qua nếu `accuracy > 50m`

**Giai đoạn 2 — Kiểm tra trạng thái vùng**
- Tính khoảng cách user–POI (Haversine)
- Hysteresis buffer 1m:
  - Enter threshold = R − 1m
  - Exit threshold = R + 1m
- Outside + distance ≤ R−1m → đưa vào **pending enter**
- Inside + distance ≥ R+1m → xác nhận **exit zone**

**Giai đoạn 3 — Debounce xác nhận enter**
- Chọn POI ưu tiên cao nhất từ Priority Queue (priority desc, distance asc)
- Chờ **3 giây** để xác nhận vào vùng thật
- Nếu vẫn trong vùng sau debounce → chuyển sang bước 4

**Giai đoạn 4 — Kiểm tra cooldown**
- Kiểm tra `shortCooldownUntil` (45 giây)
- Nếu còn cooldown → không trigger

**Giai đoạn 5 — Trigger audio**
- Set `lastTriggeredAt = now`
- Set `shortCooldownUntil = now + 45s`
- Ghi `playbackHistory`
- Gửi `TRIGGER` về main thread → phát audio

### Activity Diagram — Geofence

```mermaid
flowchart TD
    A([Nhận GPS]) --> B{accuracy > 50m?}
    B -- Có --> C([Bỏ qua])
    B -- Không --> D[Gửi LOCATION sang Worker]
    D --> E[Tính khoảng cách Haversine cho từng POI]
    E --> F{distance ≤ R−1m\nvà đang outside?}
    F -- Có --> G[Đặt pendingEnterAt = now]
    G --> H[Sắp xếp Priority Queue\npriority↓, distance↑]
    H --> I[Pop POI đứng đầu]
    I --> J{Đã qua 3s debounce\nvà vẫn trong vùng?}
    J -- Chưa --> K([Chờ tiếp])
    J -- Rồi --> L{shortCooldown\ncòn hiệu lực?}
    L -- Còn --> M([Không trigger])
    L -- Hết --> N[TRIGGER: phát audio\ncập nhật cooldown 45s\nghi playbackHistory]
    F -- Không --> O{distance ≥ R+1m\nvà đang inside?}
    O -- Có --> P[EXIT zone\nxóa pendingEnterAt]
    O -- Không --> Q([Giữ nguyên state])
```

### Sequence Diagram — Geofence

```mermaid
sequenceDiagram
    participant GPS as GPS (Main Thread)
    participant MW as MapView
    participant W as Geofence Worker
    participant DB as IndexedDB

    loop Mỗi 5 giây
        GPS->>MW: watchPosition callback {lat, lng, accuracy}
        MW->>MW: Cập nhật marker trên bản đồ
        MW->>W: postMessage LOCATION {lat, lng, accuracy}
        W->>W: accuracy > 50m?
        alt Accuracy quá thấp
            W-->>MW: LOCATION_SKIPPED
        else Accuracy hợp lệ
            W->>W: Tính distance Haversine từng POI
            W->>W: Cập nhật pending/inside/exit state
            W->>W: Sắp xếp Priority Queue
            W->>W: Đặt debounce timer 3s
        end
    end

    Note over W: Sau 3s debounce
    W->>W: Kiểm tra user vẫn trong vùng
    W->>W: Kiểm tra cooldown 45s
    W->>DB: STATE_UPDATED (lưu geofenceState)
    W->>MW: TRIGGER {poiId, distance}
    MW->>MW: openPoiDetail(poi, autoPlay=true)
    MW->>DB: Đọc localization theo userLang
    MW->>MW: playAudio()
```

---

## 7. Flow Phát Audio Tự Động

### Mô tả

Khi geofence trigger hoặc user bấm "Phát" thủ công, hệ thống xác định ngôn ngữ và chọn nguồn audio phù hợp theo 4 tầng:

- **Tầng 1 (vi/en + audioUrl)**: Phát trực tiếp từ server URL
- **Tầng 2 (IndexedDB cache)**: Phát từ Blob đã cache trên thiết bị
- **Tầng 3 (offline)**: Không thể generate — dừng lại
- **Tầng 4 (online, generate)**: Dịch text → TTS proxy → lưu server + cache client → phát

### Activity Diagram — Phát Audio Tự Động

```mermaid
flowchart TD
    A([Trigger phát audio\npoiId + userLang]) --> B{userLang là\nvi hoặc en?}

    B -- Có --> C{audioUrl có\ntrong DB?}
    C -- Có --> D[Phát từ server URL\n/audio/poiId/lang.mp3]
    C -- Không --> E

    B -- Không --> E{Có trong\nIndexedDB cache?}
    E -- Có --> F[Phát từ Blob URL\nIndexedDB]
    E -- Không --> G{Có mạng?}

    G -- Không --> H([Dừng — không thể generate offline])

    G -- Có --> I[Lấy text từ CachedPoiLocalization]
    I --> J{Có text\ncủa userLang?}
    J -- Không --> K[Gọi DeepTranslate\ndịch từ tiếng Việt]
    K --> L[Lưu text dịch vào IndexedDB]
    L --> M
    J -- Có --> M[Gọi TTS Proxy\nPOST /api/tts/proxy\npoiId + language + text]

    M --> N{Server đã có\naudio file?}
    N -- Có --> O[Trả file có sẵn\nkhông generate lại]
    N -- Không --> P[TTS generate\nlưu storage/audio\ncập nhật PoiLocalizations DB]
    P --> Q[Trả binary audio]
    O --> Q

    Q --> R[Lưu Blob vào\npoiAudios IndexedDB]
    R --> S[Phát từ Blob URL]

    D --> T([Ghi playbackHistory])
    F --> T
    S --> T
```

### Sequence Diagram — Phát Audio Tự Động

```mermaid
sequenceDiagram
    participant MW as MapView (PWA)
    participant IDB as IndexedDB
    participant API as C# API
    participant TTS as TTS Service
    participant DT as DeepTranslate

    MW->>MW: Xác định userLang từ navigator.language
    MW->>IDB: Đọc poiLocalizations[poiId-userLang]

    alt userLang = vi hoặc en và có audioUrl
        MW->>API: GET /audio/{poiId}/{lang}.mp3
        API-->>MW: MP3 binary
        MW->>MW: new Audio(url).play()

    else Đã có trong poiAudios cache
        MW->>IDB: Đọc poiAudios[poiId-userLang]
        IDB-->>MW: Blob
        MW->>MW: createObjectURL(blob).play()

    else Online, chưa có cache
        alt Chưa có text bản dịch
            MW->>IDB: Đọc poiLocalizations[poiId-vi]
            IDB-->>MW: text tiếng Việt
            MW->>API: POST /api/localize/translate {text, targetLang}
            API->>DT: POST /translate
            DT-->>API: translated_text
            API-->>MW: translated_text
            MW->>IDB: Lưu poiLocalizations[poiId-userLang]
        end

        MW->>API: POST /api/tts/proxy {poiId, language, text}\nX-Session-Token
        API->>API: Kiểm tra PoiLocalizations + file tồn tại
        alt Chưa có
            API->>TTS: POST /tts/generate {poi_id, language, text}
            TTS->>TTS: edge-tts generate MP3
            TTS->>TTS: Lưu storage/audio/{poiId}/{lang}.mp3
            TTS-->>API: {audio_url}
            API->>API: Cập nhật PoiLocalizations.AudioUrl
        end
        API-->>MW: MP3 binary
        MW->>IDB: Lưu poiAudios[poiId-userLang] = Blob
        MW->>MW: createObjectURL(blob).play()
    end

    MW->>IDB: Ghi playbackHistory
```

---

## 8. Flow Gửi / Duyệt POI

### Mô tả

Owner tạo POI → dịch text background → submit → Admin duyệt/từ chối → nếu approved thì generate TTS và đưa lên map.

### Activity Diagram — Gửi / Duyệt POI

```mermaid
flowchart TD
    A([Owner tạo POI\nnhập tên, tọa độ, mô tả]) --> B[Lưu DB: Status = Draft\nPoiLocalization vi]
    B --> C[Background: DeepTranslate\ndịch sang en, zh, ja, ru\nlưu PoiLocalizations]
    C --> D{Owner submit\nyêu cầu duyệt?}
    D -- Chưa --> E([POI ở trạng thái Draft])
    D -- Submit --> F[Status = Pending]
    F --> G[Admin xem danh sách\nPOI chờ duyệt]

    G --> H{Admin quyết định}
    H -- Từ chối --> I[Hiện popup\nnhập lý do từ chối]
    I --> J{Admin xác nhận\nhay hủy popup?}
    J -- Hủy --> G
    J -- Xác nhận --> K[Status = Rejected\nLưu lý do vào ApprovalLogs]
    K --> L([Owner nhận thông báo\ncó thể sửa và submit lại])

    H -- Duyệt --> M[Status = Approved\nLưu ApprovalLogs]
    M --> N[Background: Generate TTS\ncho vi, en và các ngôn ngữ đã dịch]
    N --> O[Lưu AudioUrl vào\nPoiLocalizations]
    O --> P[POI xuất hiện trên\nbản đồ du lịch]
    P --> Q([Bootstrap sync\ntrả POI cho tourist])
```

### Sequence Diagram — Gửi / Duyệt POI

```mermaid
sequenceDiagram
    participant OW as Owner (WebApp)
    participant API as C# API
    participant DB as SQL Server
    participant DT as DeepTranslate
    participant TTS as TTS Service
    participant AD as Admin (WebApp)
    participant MW as Mobile (PWA)

    OW->>API: POST /api/pois {name, lat, lng, description, packageId}
    API->>DB: INSERT Poi (Status=Draft)
    API->>DB: INSERT PoiLocalization {language=vi, text}
    API-->>OW: 201 Created {id, status=Draft}

    Note over API: Background task
    API->>DT: POST /translate {text, target=en}
    DT-->>API: translated_text (en)
    API->>DB: INSERT PoiLocalization {language=en}
    API->>DT: POST /translate {text, target=zh/ja/ru}
    DT-->>API: translated_text
    API->>DB: INSERT PoiLocalization {language=zh/ja/ru}

    OW->>API: PATCH /api/pois/{id}/submit
    API->>DB: UPDATE Poi SET Status=Pending
    API-->>OW: {status=Pending}

    AD->>API: GET /api/pois/pending
    API->>DB: SELECT WHERE Status=Pending
    DB-->>API: POI list
    API-->>AD: POI list + localizations

    alt Admin từ chối
        AD->>API: PATCH /api/pois/{id}/reject {note}
        API->>DB: UPDATE Status=Rejected
        API->>DB: INSERT PoiApprovalLog {action=Rejected, note}
        API-->>AD: {status=Rejected}
    else Admin duyệt
        AD->>API: PATCH /api/pois/{id}/approve
        API->>DB: UPDATE Status=Approved
        API->>DB: INSERT PoiApprovalLog {action=Approved}
        API-->>AD: {status=Approved}

        Note over API: Background: generate TTS
        loop Mỗi ngôn ngữ đã dịch
            API->>TTS: POST /tts/generate {poi_id, language, text}
            TTS->>TTS: edge-tts → MP3
            TTS-->>API: {audio_url}
            API->>DB: UPDATE PoiLocalization SET AudioUrl
        end

        MW->>API: GET /api/access/bootstrap
        API->>DB: SELECT Approved POIs + vi/en localizations
        DB-->>API: POI list
        API-->>MW: Bootstrap data (vi+en only)
        MW->>MW: Cập nhật IndexedDB + bản đồ
    end
```

---

## 9. Flow DeepTranslate

Gồm 4 tầng:

- **Tầng 1**: Online + ngôn ngữ đã có sẵn trong CachedDB (vi/en từ bootstrap) → phát trực tiếp
- **Tầng 2**: Online + ngôn ngữ chưa có → gọi DeepTranslate → dịch → lưu CachedDB client → generate TTS on-demand
- **Tầng 3**: Offline + ngôn ngữ chưa có → fallback tiếng Anh

---

## 10. Trường hợp POI đè lên nhau

Khi nhiều POI trùng vùng, hệ thống dùng **Priority Queue**:
- Sắp xếp theo `Priority` giảm dần
- Nếu Priority bằng nhau: sắp xếp theo khoảng cách tăng dần
- Chỉ trigger POI đứng đầu queue sau khi debounce pass

---

## 11. Trường hợp nhiều người cùng đứng tại 1 POI

| Rủi ro | Giải pháp |
|--------|-----------|
| TTS generate N lần cùng nội dung | `SemaphoreSlim` per `{poiId}-{language}` — lần đầu generate, lần sau dùng file cũ |
| Rate limit chặn nhầm | Rate limit theo `X-Session-Token` thay vì global |
| Audio chồng nhau | Cooldown 45s + dừng audio cũ trước khi phát mới |

---

## 12. API Endpoints

### Auth
| Method | URL | Mô tả |
|--------|-----|-------|
| POST | `/api/auth/login` | JWT cho Admin/Owner |
| POST | `/api/access/verify` | Kiểm tra session token tourist |

### QR & Thanh toán
| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/api/qr` | Danh sách QR codes |
| POST | `/api/qr` | Tạo QR mới |
| PATCH | `/api/qr/{id}/toggle` | Bật/tắt QR |
| DELETE | `/api/qr/{id}` | Xóa QR |
| POST | `/api/access/pay` | Tạo VNPay URL từ QR code |
| GET | `/api/access/callback` | VNPay callback → tạo session |
| GET | `/api/access/bootstrap` | Tải POI + audio vi/en (cần session token) |

### POI
| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/api/pois` | Danh sách POI (Owner: của mình, Admin: tất cả) |
| POST | `/api/pois` | Tạo POI |
| PUT | `/api/pois/{id}` | Cập nhật POI |
| DELETE | `/api/pois/{id}` | Xóa POI + audio files |
| PATCH | `/api/pois/{id}/submit` | Owner submit duyệt |
| GET | `/api/pois/pending` | Danh sách chờ duyệt (Admin) |
| PATCH | `/api/pois/{id}/approve` | Duyệt POI |
| PATCH | `/api/pois/{id}/reject` | Từ chối POI |

### Localization & Audio
| Method | URL | Mô tả |
|--------|-----|-------|
| POST | `/api/pois/{id}/localize` | Trigger dịch + TTS cho 1 POI |
| POST | `/api/admin/audio/bulk` | Bulk generate audio nhiều POI |
| POST | `/api/tts/proxy` | Generate TTS on-demand, lưu server (cần session token) |

### Stats
| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/api/stats/realtime` | Số session đang active |
| GET | `/api/stats/sessions?period=` | Thống kê theo kỳ |
| GET | `/api/stats/heatmap?period=` | Heatmap truy cập |
| WS | `/hubs/admin` | SignalR — push `OnlineCount` mỗi 30s |