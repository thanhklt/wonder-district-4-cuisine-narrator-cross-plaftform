"use client";
import { useEffect, useRef, useState } from "react";
import { db, type CachedPoi, type CachedPoiLocalization, type CachedPoiImage } from "@/lib/db";
import { fetchBootstrap } from "@/lib/api";

interface AudioState {
  poiId: string | null;
  playing: boolean;
}

interface PoiDetail {
  poi: CachedPoi;
  localization: CachedPoiLocalization | null;
  images: CachedPoiImage[];
}

export default function MapView() {
  const mapRef = useRef<HTMLDivElement>(null);
  const leafletMap = useRef<any>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const workerRef = useRef<Worker | null>(null);
  const userMarkerRef = useRef<any>(null);
  const sheetRef = useRef<HTMLDivElement>(null);
  const [pois, setPois] = useState<CachedPoi[]>([]);
  const [audioState, setAudioState] = useState<AudioState>({ poiId: null, playing: false });
  const [detail, setDetail] = useState<PoiDetail | null>(null);
  const [imgIndex, setImgIndex] = useState(0);
  const [userPos, setUserPos] = useState<{ lat: number; lng: number } | null>(null);
  const [gpsError, setGpsError] = useState<string | null>(null);

  const userLang = typeof navigator !== "undefined"
    ? navigator.language.split("-")[0].toLowerCase()
    : "vi";

  // ── Bootstrap ────────────────────────────────────────────
  useEffect(() => {
    async function bootstrap() {
      const cached = await db.pois.toArray();
      if (cached.length > 0) {
        setPois(cached);
        return;
      }
      try {
        const data = await fetchBootstrap();
        const poisToCache: CachedPoi[] = data.pois.map((p: any) => ({
          id: p.id, name: p.name, lat: p.lat, lng: p.lng,
          radiusMeters: p.radiusMeters, priority: p.priority,
        }));
        await db.pois.bulkPut(poisToCache);

        for (const p of data.pois) {
          for (const loc of p.localizations ?? []) {
            await db.poiLocalizations.put({
              id: `${p.id}-${loc.language}`,
              poiId: p.id, language: loc.language,
              textContent: loc.textContent, audioUrl: loc.audioUrl,
            });
          }
          for (const [i, imgUrl] of (p.images ?? []).entries()) {
            await db.poiImages.put({
              id: `${p.id}-${i}`,
              poiId: p.id, imageUrl: imgUrl, order: i,
            });
          }
        }
        setPois(poisToCache);
      } catch (e) {
        console.error("Bootstrap failed", e);
      }
    }
    bootstrap();
  }, []);

  // ── Init Leaflet ─────────────────────────────────────────
  useEffect(() => {
    if (!mapRef.current || leafletMap.current) return;
    import("leaflet").then((L) => {
      leafletMap.current = L.map(mapRef.current!, {
        zoomControl: false,
        attributionControl: false,
      }).setView([10.7563, 106.7016], 17);

      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 20,
      }).addTo(leafletMap.current);

      L.control.zoom({ position: "bottomright" }).addTo(leafletMap.current);
      L.control.attribution({ position: "bottomleft", prefix: "© OpenStreetMap" })
        .addTo(leafletMap.current);
    });
    import("leaflet/dist/leaflet.css");
  }, []);

  // ── POI markers ──────────────────────────────────────────
  useEffect(() => {
    if (!leafletMap.current || pois.length === 0) return;
    import("leaflet").then((L) => {
      pois.forEach((poi) => {
        L.circle([poi.lat, poi.lng], {
          radius: poi.radiusMeters,
          color: "#f97316",
          fillColor: "#f97316",
          fillOpacity: 0.08,
          weight: 1.5,
          dashArray: "5 5",
        }).addTo(leafletMap.current);

        const icon = L.divIcon({
          className: "",
          html: `
            <div style="position:relative;width:40px;height:48px;">
              <div style="
                position:absolute;bottom:0;left:50%;transform:translateX(-50%);
                width:40px;height:40px;
                background:#f97316;
                border-radius:50% 50% 50% 0;
                transform:translateX(-50%) rotate(-45deg);
                border:3px solid #fff;
                box-shadow:0 3px 10px rgba(0,0,0,0.3);
              "></div>
              <span style="
                position:absolute;bottom:8px;left:50%;
                transform:translateX(-50%);
                font-size:18px;line-height:1;
              ">🍽️</span>
            </div>`,
          iconSize: [40, 48],
          iconAnchor: [20, 48],
          popupAnchor: [0, -48],
        });

        const marker = L.marker([poi.lat, poi.lng], { icon }).addTo(leafletMap.current);
        marker.on("click", () => openPoiDetail(poi, false));
      });
    });
  }, [pois]);

  // ── GPS: lưu vị trí vào state ────────────────────────────
  useEffect(() => {
    if (!("geolocation" in navigator)) {
      setGpsError("Thiết bị không hỗ trợ GPS");
      return;
    }
    const watchId = navigator.geolocation.watchPosition(
      (pos) => {
        setUserPos({ lat: pos.coords.latitude, lng: pos.coords.longitude });
        setGpsError(null);
      },
      (err) => setGpsError(err.message),
      { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 },
    );
    return () => navigator.geolocation.clearWatch(watchId);
  }, []);

  // ── Cập nhật marker khi userPos thay đổi (sau khi map đã sẵn sàng) ──
  useEffect(() => {
    if (!userPos) return;
    updateUserMarker(userPos.lat, userPos.lng);
  }, [userPos]);

  // ── Geofence worker (chỉ lo trigger audio, không lo vị trí) ─────────
  useEffect(() => {
    if (pois.length === 0) return;
    const worker = new Worker(new URL("../workers/geofence.worker.ts", import.meta.url));
    workerRef.current = worker;
    worker.postMessage({ type: "INIT", payload: { pois } });
    worker.onmessage = (e) => {
      const { type, poiId } = e.data;
      if (type === "TRIGGER") {
        const poi = pois.find((p) => p.id === poiId);
        if (poi) openPoiDetail(poi, true);
      }
    };
    return () => worker.terminate();
  }, [pois]);

  // ── Actions ──────────────────────────────────────────────
  async function openPoiDetail(poi: CachedPoi, autoPlay: boolean) {
    const loc =
      (await db.poiLocalizations.get(`${poi.id}-${userLang}`)) ??
      (await db.poiLocalizations.get(`${poi.id}-en`)) ??
      (await db.poiLocalizations.get(`${poi.id}-vi`)) ??
      null;
    const images = await db.poiImages.where("poiId").equals(poi.id).sortBy("order");
    setDetail({ poi, localization: loc, images });
    setImgIndex(0);
    if (autoPlay && loc?.audioUrl) playAudio(poi, loc);
    // Pan map up so marker isn't hidden behind sheet
    import("leaflet").then(() => {
      if (!leafletMap.current) return;
      const pt = leafletMap.current.latLngToContainerPoint([poi.lat, poi.lng]);
      const offset = leafletMap.current.containerPointToLatLng([pt.x, pt.y + 120]);
      leafletMap.current.panTo(offset, { animate: true, duration: 0.3 });
    });
  }

  function closeDetail() {
    setDetail(null);
    stopAudio();
  }

  function updateUserMarker(lat: number, lng: number) {
    // import("leaflet") được cache sau lần đầu — không lo hiệu năng
    import("leaflet").then((L) => {
      if (!leafletMap.current) return;
      if (userMarkerRef.current) {
        userMarkerRef.current.setLatLng([lat, lng]);
      } else {
        userMarkerRef.current = L.circleMarker([lat, lng], {
          radius: 9, color: "#fff", fillColor: "#3b82f6", fillOpacity: 1, weight: 3,
        }).addTo(leafletMap.current);
      }
    });
  }

  function centerOnUser() {
    if (!leafletMap.current || !userMarkerRef.current) return;
    leafletMap.current.setView(userMarkerRef.current.getLatLng(), 18, { animate: true });
  }

  async function playAudio(poi: CachedPoi, loc?: CachedPoiLocalization | null) {
    if (audioRef.current) { audioRef.current.pause(); audioRef.current = null; }
    const locData = loc ??
      (await db.poiLocalizations.get(`${poi.id}-${userLang}`)) ??
      (await db.poiLocalizations.get(`${poi.id}-en`));
    if (!locData?.audioUrl) return;
    const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";
    const audio = new Audio(`${apiUrl}${locData.audioUrl}`);
    audioRef.current = audio;
    audio.play().catch(() => {});
    setAudioState({ poiId: poi.id, playing: true });
    audio.onended = () => setAudioState({ poiId: null, playing: false });
  }

  function stopAudio() {
    if (audioRef.current) { audioRef.current.pause(); audioRef.current = null; }
    setAudioState({ poiId: null, playing: false });
  }

  const isPlaying = audioState.playing && detail !== null && audioState.poiId === detail.poi.id;

  return (
    <div className="relative w-full h-full overflow-hidden">
      {/* Map */}
      <div ref={mapRef} className="w-full h-full" />

      {/* GPS error banner */}
      {gpsError && (
        <div className="absolute top-3 left-3 right-3 z-[1001] bg-red-50 border border-red-200 rounded-xl px-3 py-2 flex items-start gap-2">
          <span className="text-red-500 text-base mt-0.5 flex-shrink-0">⚠️</span>
          <div className="min-w-0">
            <p className="text-xs font-semibold text-red-700">Không lấy được vị trí GPS</p>
            <p className="text-xs text-red-500 mt-0.5 break-words">{gpsError}</p>
            {gpsError.toLowerCase().includes("secure") || gpsError.toLowerCase().includes("permission") || gpsError.includes("1") ? (
              <p className="text-xs text-red-400 mt-1">
                Chrome chặn GPS trên HTTP. Mở <b>chrome://flags/#unsafely-treat-insecure-origin-as-secure</b> → thêm <b>http://192.168.1.198:3000</b> → khởi động lại Chrome.
              </p>
            ) : null}
          </div>
        </div>
      )}

      {/* Center-on-me button */}
      <button
        onClick={centerOnUser}
        className="absolute z-[1000] bg-white rounded-full shadow-lg border border-gray-200 flex items-center justify-center text-blue-500"
        style={{ bottom: detail ? "calc(65vh + 12px)" : "80px", right: "12px", width: 44, height: 44, transition: "bottom 0.3s" }}
        title="Về vị trí của tôi"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2">
          <circle cx="12" cy="12" r="3" />
          <path d="M12 2v3M12 19v3M2 12h3M19 12h3" />
          <circle cx="12" cy="12" r="9" strokeOpacity=".3" />
        </svg>
      </button>

      {/* Backdrop */}
      {detail && (
        <div
          className="absolute inset-0 z-[1001]"
          style={{ background: "transparent" }}
          onClick={closeDetail}
        />
      )}

      {/* Bottom sheet */}
      <div
        ref={sheetRef}
        className="absolute inset-x-0 bottom-0 z-[1002] bg-white rounded-t-3xl shadow-2xl flex flex-col"
        style={{
          maxHeight: "65vh",
          transform: detail ? "translateY(0)" : "translateY(100%)",
          transition: "transform 0.32s cubic-bezier(0.32,0.72,0,1)",
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {detail && (
          <>
            {/* Handle */}
            <div className="flex justify-center pt-3 pb-0 flex-shrink-0">
              <div className="w-10 h-1 bg-gray-200 rounded-full" />
            </div>

            {/* Header */}
            <div className="flex items-start px-5 pt-3 pb-2 gap-2 flex-shrink-0">
              <div className="flex-1 min-w-0">
                <h2 className="text-base font-bold text-gray-900 leading-snug">{detail.poi.name}</h2>
                <div className="flex items-center gap-1 mt-0.5">
                  <span className="text-orange-500 text-xs">📍</span>
                  <span className="text-xs text-orange-500 font-medium">Phố ẩm thực Vĩnh Khánh</span>
                </div>
              </div>
              <button
                onClick={closeDetail}
                className="flex-shrink-0 w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-gray-500 text-lg leading-none"
              >
                ×
              </button>
            </div>

            {/* Scrollable content */}
            <div className="flex-1 overflow-y-auto overscroll-contain">
              {/* Images */}
              {detail.images.length > 0 && (
                <div className="px-5 pb-3">
                  <div className="relative rounded-2xl overflow-hidden bg-gray-100" style={{ height: 180 }}>
                    <img
                      src={detail.images[imgIndex]?.imageUrl}
                      alt={detail.poi.name}
                      className="w-full h-full object-cover"
                    />
                    {detail.images.length > 1 && (
                      <>
                        <button
                          onClick={() => setImgIndex((i) => Math.max(0, i - 1))}
                          disabled={imgIndex === 0}
                          className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/40 text-white rounded-full w-8 h-8 flex items-center justify-center disabled:opacity-30"
                        >‹</button>
                        <button
                          onClick={() => setImgIndex((i) => Math.min(detail.images.length - 1, i + 1))}
                          disabled={imgIndex === detail.images.length - 1}
                          className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/40 text-white rounded-full w-8 h-8 flex items-center justify-center disabled:opacity-30"
                        >›</button>
                        <div className="absolute bottom-2 left-1/2 -translate-x-1/2 flex gap-1">
                          {detail.images.map((_, i) => (
                            <div
                              key={i}
                              className={`rounded-full transition-all ${i === imgIndex ? "w-4 h-1.5 bg-white" : "w-1.5 h-1.5 bg-white/50"}`}
                            />
                          ))}
                        </div>
                      </>
                    )}
                  </div>
                </div>
              )}

              {/* Description */}
              <div className="px-5 pb-4">
                {detail.localization?.textContent ? (
                  <p className="text-sm text-gray-700 leading-relaxed">
                    {detail.localization.textContent}
                  </p>
                ) : (
                  <p className="text-sm text-gray-400 italic">Chưa có thông tin thuyết minh.</p>
                )}
              </div>
            </div>

            {/* Audio bar */}
            <div className="flex-shrink-0 border-t border-gray-100 px-5 py-3 flex items-center gap-3 bg-white">
              {isPlaying ? (
                <>
                  <div className="w-9 h-9 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
                    <span className="text-lg animate-pulse">🔊</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-gray-800 truncate">Đang phát thuyết minh</p>
                    <div className="flex gap-0.5 mt-1">
                      {[1,2,3,4,5].map((i) => (
                        <div
                          key={i}
                          className="w-0.5 bg-orange-400 rounded-full animate-pulse"
                          style={{ height: `${8 + (i % 3) * 4}px`, animationDelay: `${i * 0.1}s` }}
                        />
                      ))}
                    </div>
                  </div>
                  <button
                    onClick={stopAudio}
                    className="flex-shrink-0 px-4 py-2 rounded-full bg-gray-100 text-sm font-medium text-gray-600"
                  >
                    Dừng
                  </button>
                </>
              ) : (
                <>
                  <div className="w-9 h-9 rounded-full bg-gray-100 flex items-center justify-center flex-shrink-0">
                    <span className="text-lg">🎙️</span>
                  </div>
                  <p className="flex-1 text-xs text-gray-500">
                    {detail.localization?.audioUrl
                      ? "Nghe thuyết minh tự động"
                      : "Chưa có audio"}
                  </p>
                  <button
                    onClick={() => playAudio(detail.poi, detail.localization)}
                    disabled={!detail.localization?.audioUrl}
                    className="flex-shrink-0 px-4 py-2 rounded-full bg-orange-500 disabled:bg-gray-100 text-white disabled:text-gray-400 text-sm font-semibold"
                  >
                    Phát
                  </button>
                </>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
