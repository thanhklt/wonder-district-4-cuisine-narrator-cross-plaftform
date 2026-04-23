"use client";
import Image from "next/image";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { CircleMarker, LayerGroup, Map as LeafletMap } from "leaflet";
import {
  db,
  type AudioPlaybackHistory,
  type CachedPoi,
  type CachedPoiImage,
  type CachedPoiLocalization,
  type PoiGeofenceState,
} from "@/lib/db";
import { fetchBootstrap, getSessionToken, translateText } from "@/lib/api";

const GPS_POLL_INTERVAL_MS = 5000;
const GPS_TIMEOUT_MS = 15000;

interface AudioState {
  poiId: string | null;
  playing: boolean;
  generating: boolean;
}

interface PoiDetail {
  poi: CachedPoi;
  localization: CachedPoiLocalization | null;
  images: CachedPoiImage[];
}

interface BootstrapPoiLocalization {
  language: string;
  textContent: string;
  audioUrl?: string;
}

interface BootstrapPoi {
  id: string;
  name: string;
  lat: number;
  lng: number;
  radiusMeters: number;
  priority: number;
  localizations?: BootstrapPoiLocalization[];
  images?: string[];
}

interface BootstrapResponse {
  pois: BootstrapPoi[];
}

interface WorkerTriggerMessage {
  type: "TRIGGER";
  poiId: string;
  distance: number;
}

interface WorkerExitMessage {
  type: "EXIT";
  poiId: string;
}

interface WorkerStateUpdatedMessage {
  type: "STATE_UPDATED";
  payload: PoiGeofenceState;
}

interface WorkerLocationSkippedMessage {
  type: "LOCATION_SKIPPED";
  reason: "LOW_ACCURACY";
  accuracy: number;
  recordedAt?: number;
}

type WorkerMessage =
  | WorkerTriggerMessage
  | WorkerExitMessage
  | WorkerStateUpdatedMessage
  | WorkerLocationSkippedMessage;

export default function MapView() {
  const mapRef = useRef<HTMLDivElement>(null);
  const leafletMap = useRef<LeafletMap | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const workerRef = useRef<Worker | null>(null);
  const userMarkerRef = useRef<CircleMarker | null>(null);
  const poiLayerGroupRef = useRef<LayerGroup | null>(null);
  const sheetRef = useRef<HTMLDivElement>(null);
  const gpsTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const gpsPollRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isGpsRequestInFlightRef = useRef(false);
  const [pois, setPois] = useState<CachedPoi[]>([]);
  const [audioState, setAudioState] = useState<AudioState>({ poiId: null, playing: false, generating: false });
  const [detail, setDetail] = useState<PoiDetail | null>(null);
  const [imgIndex, setImgIndex] = useState(0);
  const [userPos, setUserPos] = useState<{ lat: number; lng: number } | null>(null);
  const [gpsError, setGpsError] = useState<string | null>(null);

  const userLang = useMemo(
    () => (typeof navigator !== "undefined" ? navigator.language.split("-")[0].toLowerCase() : "vi"),
    [],
  );

  const trackPlaybackHistory = useCallback(async (poiId: string) => {
    const history: AudioPlaybackHistory = {
      id: `${poiId}-${Date.now()}`,
      poiId,
      language: userLang,
      playedAt: Date.now(),
    };
    await db.playbackHistory.put(history);
  }, [userLang]);

  const playFromUrl = useCallback(async (url: string, poiId: string) => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }

    const audio = new Audio(url);
    audioRef.current = audio;
    await trackPlaybackHistory(poiId);
    void audio.play().catch(() => {});
    setAudioState({ poiId, playing: true, generating: false });
    audio.onended = () => setAudioState({ poiId: null, playing: false, generating: false });
  }, [trackPlaybackHistory]);

  const playFromBlob = useCallback(async (blob: Blob, poiId: string) => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }

    const blobUrl = URL.createObjectURL(blob);
    const audio = new Audio(blobUrl);
    audioRef.current = audio;
    await trackPlaybackHistory(poiId);
    void audio.play().catch(() => {});
    setAudioState({ poiId, playing: true, generating: false });
    audio.onended = () => {
      URL.revokeObjectURL(blobUrl);
      setAudioState({ poiId: null, playing: false, generating: false });
    };
  }, [trackPlaybackHistory]);

  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "";

  const playAudio = useCallback(async (poi: CachedPoi, loc?: CachedPoiLocalization | null) => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }

    if (userLang === "vi" || userLang === "en") {
      const locData =
        loc ??
        (await db.poiLocalizations.get(`${poi.id}-${userLang}`)) ??
        (await db.poiLocalizations.get(`${poi.id}-en`)) ??
        (await db.poiLocalizations.get(`${poi.id}-vi`));

      if (locData?.audioUrl) {
        await playFromUrl(`${apiUrl}${locData.audioUrl}`, poi.id);
        return;
      }
    }

    const cached = await db.poiAudios.get(`${poi.id}-${userLang}`);
    if (cached) {
      await playFromBlob(cached.audioBlob, poi.id);
      return;
    }

    if (!navigator.onLine) {
      setAudioState({ poiId: null, playing: false, generating: false });
      return;
    }

    setAudioState({ poiId: poi.id, playing: false, generating: true });

    try {
      let text: string | undefined;
      const langLoc = await db.poiLocalizations.get(`${poi.id}-${userLang}`);
      if (langLoc?.textContent) {
        text = langLoc.textContent;
      } else {
        const viLoc = await db.poiLocalizations.get(`${poi.id}-vi`);
        if (!viLoc?.textContent) {
          setAudioState({ poiId: null, playing: false, generating: false });
          return;
        }

        text = userLang === "vi" ? viLoc.textContent : await translateText(viLoc.textContent, userLang);
        if (userLang !== "vi") {
          await db.poiLocalizations.put({
            id: `${poi.id}-${userLang}`,
            poiId: poi.id,
            language: userLang,
            textContent: text,
          });
        }
      }

      const res = await fetch("/api/tts/proxy", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Session-Token": getSessionToken() ?? "",
        },
        body: JSON.stringify({ poiId: poi.id, language: userLang, text }),
      });

      if (!res.ok) {
        throw new Error(`TTS proxy ${res.status}`);
      }

      const audioBlob = await res.blob();
      await db.poiAudios.put({
        id: `${poi.id}-${userLang}`,
        poiId: poi.id,
        language: userLang,
        audioBlob,
      });
      await playFromBlob(audioBlob, poi.id);
    } catch (error) {
      console.error("playAudio failed:", error);
      setAudioState({ poiId: null, playing: false, generating: false });
    }
  }, [apiUrl, playFromBlob, playFromUrl, userLang]);

  const openPoiDetail = useCallback(async (poi: CachedPoi, autoPlay: boolean) => {
    const loc =
      (await db.poiLocalizations.get(`${poi.id}-${userLang}`)) ??
      (await db.poiLocalizations.get(`${poi.id}-en`)) ??
      (await db.poiLocalizations.get(`${poi.id}-vi`)) ??
      null;
    const images = await db.poiImages.where("poiId").equals(poi.id).sortBy("order");
    setDetail({ poi, localization: loc, images });
    setImgIndex(0);

    if (autoPlay) {
      await playAudio(poi, loc);
    }

    import("leaflet").then(() => {
      if (!leafletMap.current) {
        return;
      }

      const pt = leafletMap.current.latLngToContainerPoint([poi.lat, poi.lng]);
      const offset = leafletMap.current.containerPointToLatLng([pt.x, pt.y + 120]);
      leafletMap.current.panTo(offset, { animate: true, duration: 0.3 });
    });
  }, [playAudio, userLang]);

  const updateUserMarker = useCallback((lat: number, lng: number) => {
    import("leaflet").then((L) => {
      if (!leafletMap.current) {
        return;
      }

      if (userMarkerRef.current) {
        userMarkerRef.current.setLatLng([lat, lng]);
      } else {
        userMarkerRef.current = L.circleMarker([lat, lng], {
          radius: 9,
          color: "#fff",
          fillColor: "#3b82f6",
          fillOpacity: 1,
          weight: 3,
        }).addTo(leafletMap.current);
      }
    });
  }, []);

  const pushLocationToWorker = useCallback((lat: number, lng: number, accuracy: number) => {
    workerRef.current?.postMessage({
      type: "LOCATION",
      payload: { lat, lng, accuracy, recordedAt: Date.now() },
    });
  }, []);

  useEffect(() => {
    async function bootstrap() {
      const cached = await db.pois.toArray();
      if (cached.length > 0) {
        setPois(cached);
      }

      if (!navigator.onLine) {
        return;
      }

      try {
        const data = (await fetchBootstrap()) as BootstrapResponse;
        const serverIds = new Set<string>(data.pois.map((poi) => poi.id));
        const cachedIds = (await db.pois.toArray()).map((poi) => poi.id);
        const removedIds = cachedIds.filter((id) => !serverIds.has(id));

        if (removedIds.length > 0) {
          await db.pois.bulkDelete(removedIds);
          await Promise.all(removedIds.map((id) => db.poiLocalizations.where("poiId").equals(id).delete()));
          await Promise.all(removedIds.map((id) => db.poiImages.where("poiId").equals(id).delete()));
          await Promise.all(removedIds.map((id) => db.geofenceState.delete(id)));
        }

        const poisToCache: CachedPoi[] = data.pois.map((poi) => ({
          id: poi.id,
          name: poi.name,
          lat: poi.lat,
          lng: poi.lng,
          radiusMeters: poi.radiusMeters,
          priority: poi.priority,
        }));
        await db.pois.bulkPut(poisToCache);

        for (const poi of data.pois) {
          for (const localization of poi.localizations ?? []) {
            await db.poiLocalizations.put({
              id: `${poi.id}-${localization.language}`,
              poiId: poi.id,
              language: localization.language,
              textContent: localization.textContent,
              audioUrl: localization.audioUrl,
            });
          }

          for (const [index, imageUrl] of (poi.images ?? []).entries()) {
            await db.poiImages.put({
              id: `${poi.id}-${index}`,
              poiId: poi.id,
              imageUrl,
              order: index,
            });
          }
        }

        setPois(poisToCache);
      } catch (error) {
        console.error("Sync failed, using cache", error);
      }
    }

    void bootstrap();
  }, []);

  useEffect(() => {
    if (!mapRef.current || leafletMap.current) {
      return;
    }

    import("leaflet").then((L) => {
      leafletMap.current = L.map(mapRef.current!, {
        zoomControl: false,
        attributionControl: false,
      }).setView([10.7563, 106.7016], 17);

      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 20,
      }).addTo(leafletMap.current);

      L.control.zoom({ position: "bottomright" }).addTo(leafletMap.current);
      L.control.attribution({ position: "bottomleft", prefix: "© OpenStreetMap" }).addTo(leafletMap.current);
    });

    void import("leaflet/dist/leaflet.css");
  }, []);

  useEffect(() => {
    if (!leafletMap.current || pois.length === 0) {
      return;
    }

    import("leaflet").then((L) => {
      if (poiLayerGroupRef.current) {
        poiLayerGroupRef.current.clearLayers();
      } else {
        poiLayerGroupRef.current = L.layerGroup().addTo(leafletMap.current!);
      }

      for (const poi of pois) {
        L.circle([poi.lat, poi.lng], {
          radius: poi.radiusMeters,
          color: "#f97316",
          fillColor: "#f97316",
          fillOpacity: 0.08,
          weight: 1.5,
          dashArray: "5 5",
        }).addTo(poiLayerGroupRef.current);

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

        const marker = L.marker([poi.lat, poi.lng], { icon }).addTo(poiLayerGroupRef.current);
        marker.on("click", () => {
          void openPoiDetail(poi, false);
        });
      }
    });
  }, [openPoiDetail, pois]);

  useEffect(() => {
    if (typeof navigator === "undefined" || !("geolocation" in navigator)) {
      const timer = setTimeout(() => {
        setGpsError("Thiet bi khong ho tro GPS");
      }, 0);
      return () => clearTimeout(timer);
    }

    const setTimeoutHint = () => {
      if (gpsTimeoutRef.current) {
        clearTimeout(gpsTimeoutRef.current);
      }
      gpsTimeoutRef.current = setTimeout(() => {
        setGpsError("Het thoi gian cho GPS. Ung dung se tiep tuc thu lai sau moi 5 giay.");
      }, GPS_TIMEOUT_MS);
    };

    const scheduleNextPoll = () => {
      if (gpsPollRef.current) {
        clearTimeout(gpsPollRef.current);
      }
      gpsPollRef.current = setTimeout(requestCurrentPosition, GPS_POLL_INTERVAL_MS);
    };

    const requestCurrentPosition = () => {
      if (isGpsRequestInFlightRef.current) {
        scheduleNextPoll();
        return;
      }

      isGpsRequestInFlightRef.current = true;
      setTimeoutHint();

      navigator.geolocation.getCurrentPosition(
        (position) => {
          isGpsRequestInFlightRef.current = false;
          if (gpsTimeoutRef.current) {
            clearTimeout(gpsTimeoutRef.current);
          }

          const { latitude: lat, longitude: lng, accuracy } = position.coords;
          setUserPos({ lat, lng });
          setGpsError(null);
          pushLocationToWorker(lat, lng, accuracy);
          scheduleNextPoll();
        },
        (error) => {
          isGpsRequestInFlightRef.current = false;
          if (gpsTimeoutRef.current) {
            clearTimeout(gpsTimeoutRef.current);
          }

          const hints: Record<number, string> = {
            1: "Vui long cap quyen truy cap vi tri trong cai dat trinh duyet.",
            2: "Khong the xac dinh vi tri. Kiem tra GPS va Location Services da bat chua.",
            3: "Het thoi gian cho GPS. Ung dung se tiep tuc thu lai sau moi 5 giay.",
          };
          setGpsError(hints[error.code] ?? error.message);
          scheduleNextPoll();
        },
        { enableHighAccuracy: true, maximumAge: 0, timeout: GPS_TIMEOUT_MS },
      );
    };

    requestCurrentPosition();

    return () => {
      if (gpsTimeoutRef.current) {
        clearTimeout(gpsTimeoutRef.current);
      }
      if (gpsPollRef.current) {
        clearTimeout(gpsPollRef.current);
      }
    };
  }, [pushLocationToWorker]);

  useEffect(() => {
    if (!userPos) {
      return;
    }

    updateUserMarker(userPos.lat, userPos.lng);
  }, [updateUserMarker, userPos]);

  useEffect(() => {
    let cancelled = false;

    async function initWorker() {
      const savedStates = await db.geofenceState.toArray();
      if (cancelled) {
        return;
      }

      const worker = new Worker(new URL("../workers/geofence.worker.ts", import.meta.url));
      workerRef.current = worker;
      worker.postMessage({ type: "INIT", payload: { pois, states: savedStates } });
      worker.onmessage = (event: MessageEvent<WorkerMessage>) => {
        const message = event.data;

        if (message.type === "TRIGGER") {
          const poi = pois.find((item) => item.id === message.poiId);
          if (poi) {
            void openPoiDetail(poi, true);
          }
          return;
        }

        if (message.type === "STATE_UPDATED") {
          void db.geofenceState.put(message.payload);
          return;
        }

        if (message.type === "LOCATION_SKIPPED") {
          setGpsError(`Do chinh xac GPS hien tai qua thap (${Math.round(message.accuracy)}m), dang cho mau tot hon.`);
        }
      };
    }

    void initWorker();

    return () => {
      cancelled = true;
      workerRef.current?.terminate();
      workerRef.current = null;
    };
  }, [openPoiDetail, pois]);

  useEffect(() => {
    if (!workerRef.current) {
      return;
    }

    workerRef.current.postMessage({ type: "UPDATE_POIS", payload: { pois } });
  }, [pois]);

  function closeDetail() {
    setDetail(null);
    stopAudio();
  }

  function centerOnUser() {
    if (!leafletMap.current || !userMarkerRef.current) {
      return;
    }

    leafletMap.current.setView(userMarkerRef.current.getLatLng(), 18, { animate: true });
  }

  function stopAudio() {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }
    setAudioState({ poiId: null, playing: false, generating: false });
  }

  const isCurrentPoi = detail !== null && audioState.poiId === detail.poi.id;
  const isPlaying = audioState.playing && isCurrentPoi;
  const isGenerating = audioState.generating && isCurrentPoi;

  return (
    <div className="relative w-full h-full overflow-hidden">
      <div ref={mapRef} className="w-full h-full" />

      {gpsError && (
        <div className="absolute top-3 left-3 right-3 z-[1001] bg-red-50 border border-red-200 rounded-xl px-3 py-2 flex items-start gap-2">
          <span className="text-red-500 text-base mt-0.5 flex-shrink-0">⚠️</span>
          <div className="min-w-0">
            <p className="text-xs font-semibold text-red-700">Khong lay duoc vi tri GPS</p>
            <p className="text-xs text-red-500 mt-0.5 break-words">{gpsError}</p>
          </div>
        </div>
      )}

      <button
        onClick={centerOnUser}
        className="absolute z-[1000] bg-white rounded-full shadow-lg border border-gray-200 flex items-center justify-center text-blue-500"
        style={{ bottom: detail ? "calc(65vh + 12px)" : "80px", right: "12px", width: 44, height: 44, transition: "bottom 0.3s" }}
        title="Ve vi tri cua toi"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2">
          <circle cx="12" cy="12" r="3" />
          <path d="M12 2v3M12 19v3M2 12h3M19 12h3" />
          <circle cx="12" cy="12" r="9" strokeOpacity=".3" />
        </svg>
      </button>

      {detail && (
        <div className="absolute inset-0 z-[1001]" style={{ background: "transparent" }} onClick={closeDetail} />
      )}

      <div
        ref={sheetRef}
        className="absolute inset-x-0 bottom-0 z-[1002] bg-white rounded-t-3xl shadow-2xl flex flex-col"
        style={{
          maxHeight: "65vh",
          transform: detail ? "translateY(0)" : "translateY(100%)",
          transition: "transform 0.32s cubic-bezier(0.32,0.72,0,1)",
        }}
        onClick={(event) => event.stopPropagation()}
      >
        {detail && (
          <>
            <div className="flex justify-center pt-3 pb-0 flex-shrink-0">
              <div className="w-10 h-1 bg-gray-200 rounded-full" />
            </div>

            <div className="flex items-start px-5 pt-3 pb-2 gap-2 flex-shrink-0">
              <div className="flex-1 min-w-0">
                <h2 className="text-base font-bold text-gray-900 leading-snug">{detail.poi.name}</h2>
                <div className="flex items-center gap-1 mt-0.5">
                  <span className="text-orange-500 text-xs">📍</span>
                  <span className="text-xs text-orange-500 font-medium">Pho am thuc Vinh Khanh</span>
                </div>
              </div>
              <button
                onClick={closeDetail}
                className="flex-shrink-0 w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-gray-500 text-lg leading-none"
              >
                ×
              </button>
            </div>

            <div className="flex-1 overflow-y-auto overscroll-contain">
              {detail.images.length > 0 && (
                <div className="px-5 pb-3">
                  <div className="relative rounded-2xl overflow-hidden bg-gray-100" style={{ height: 180 }}>
                    <Image
                      src={detail.images[imgIndex]?.imageUrl}
                      alt={detail.poi.name}
                      fill
                      sizes="100vw"
                      unoptimized
                      className="object-cover"
                    />
                    {detail.images.length > 1 && (
                      <>
                        <button
                          onClick={() => setImgIndex((current) => Math.max(0, current - 1))}
                          disabled={imgIndex === 0}
                          className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/40 text-white rounded-full w-8 h-8 flex items-center justify-center disabled:opacity-30"
                        >
                          ‹
                        </button>
                        <button
                          onClick={() => setImgIndex((current) => Math.min(detail.images.length - 1, current + 1))}
                          disabled={imgIndex === detail.images.length - 1}
                          className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/40 text-white rounded-full w-8 h-8 flex items-center justify-center disabled:opacity-30"
                        >
                          ›
                        </button>
                        <div className="absolute bottom-2 left-1/2 -translate-x-1/2 flex gap-1">
                          {detail.images.map((_, index) => (
                            <div
                              key={index}
                              className={`rounded-full transition-all ${index === imgIndex ? "w-4 h-1.5 bg-white" : "w-1.5 h-1.5 bg-white/50"}`}
                            />
                          ))}
                        </div>
                      </>
                    )}
                  </div>
                </div>
              )}

              <div className="px-5 pb-4">
                {detail.localization?.textContent ? (
                  <p className="text-sm text-gray-700 leading-relaxed">{detail.localization.textContent}</p>
                ) : (
                  <p className="text-sm text-gray-400 italic">Chua co thong tin thuyet minh.</p>
                )}
              </div>
            </div>

            <div className="flex-shrink-0 border-t border-gray-100 px-5 py-3 flex items-center gap-3 bg-white">
              {isPlaying ? (
                <>
                  <div className="w-9 h-9 rounded-full bg-orange-100 flex items-center justify-center flex-shrink-0">
                    <span className="text-lg animate-pulse">🔊</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-gray-800 truncate">Dang phat thuyet minh</p>
                    <div className="flex gap-0.5 mt-1">
                      {[1, 2, 3, 4, 5].map((index) => (
                        <div
                          key={index}
                          className="w-0.5 bg-orange-400 rounded-full animate-pulse"
                          style={{ height: `${8 + (index % 3) * 4}px`, animationDelay: `${index * 0.1}s` }}
                        />
                      ))}
                    </div>
                  </div>
                  <button
                    onClick={stopAudio}
                    className="flex-shrink-0 px-4 py-2 rounded-full bg-gray-100 text-sm font-medium text-gray-600"
                  >
                    Dung
                  </button>
                </>
              ) : isGenerating ? (
                <>
                  <div className="w-9 h-9 rounded-full bg-blue-50 flex items-center justify-center flex-shrink-0">
                    <span className="text-lg animate-spin">⚙️</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-gray-800">Dang tao audio...</p>
                    <p className="text-xs text-gray-400 mt-0.5">Dich va tong hop giong noi</p>
                  </div>
                  <button
                    onClick={stopAudio}
                    className="flex-shrink-0 px-4 py-2 rounded-full bg-gray-100 text-sm font-medium text-gray-500"
                  >
                    Huy
                  </button>
                </>
              ) : (
                <>
                  <div className="w-9 h-9 rounded-full bg-gray-100 flex items-center justify-center flex-shrink-0">
                    <span className="text-lg">🎙️</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs text-gray-700 font-medium">Nghe thuyet minh</p>
                    <p className="text-xs text-gray-400 mt-0.5">
                      {detail.localization?.audioUrl ? "San sang phat" : "Se tao audio khi bam Phat"}
                    </p>
                  </div>
                  <button
                    onClick={() => {
                      void playAudio(detail.poi, detail.localization);
                    }}
                    className="flex-shrink-0 px-4 py-2 rounded-full bg-orange-500 text-white text-sm font-semibold hover:bg-orange-600"
                  >
                    Phat
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
