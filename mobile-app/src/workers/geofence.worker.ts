// Geofence engine chạy trong Web Worker, tách biệt khỏi main thread

const POLL_INTERVAL_MS = 5000;
const DEBOUNCE_MS = 3000;
const BUFFER_M = 1;
const SHORT_COOLDOWN_MS = 45_000;
const LONG_COOLDOWN_MS = 15 * 60_000;
const MIN_ACCURACY_M = 50;

interface Poi {
  id: string;
  lat: number;
  lng: number;
  radiusMeters: number;
  priority: number;
}

interface GeofenceState {
  isInsideZone: boolean;
  pendingEnterAt?: number;
  lastTriggeredAt?: number;
  shortCooldownUntil?: number;
  longCooldownUntil?: number;
  lastExitAt?: number;
}

let pois: Poi[] = [];
const states = new Map<string, GeofenceState>();
let watchId: number | null = null;

function toRad(deg: number) { return deg * Math.PI / 180; }

function distanceMeters(lat1: number, lng1: number, lat2: number, lng2: number): number {
  const R = 6371000;
  const dLat = toRad(lat2 - lat1);
  const dLng = toRad(lng2 - lng1);
  const a = Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng / 2) ** 2;
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function getState(poiId: string): GeofenceState {
  if (!states.has(poiId)) states.set(poiId, { isInsideZone: false });
  return states.get(poiId)!;
}

function processLocation(lat: number, lng: number) {
  const now = Date.now();
  const candidates: { poi: Poi; distance: number }[] = [];

  for (const poi of pois) {
    const dist = distanceMeters(lat, lng, poi.lat, poi.lng);
    const enterThreshold = poi.radiusMeters - BUFFER_M;
    const exitThreshold = poi.radiusMeters + BUFFER_M;
    const state = getState(poi.id);

    if (!state.isInsideZone && dist <= enterThreshold) {
      // Stage 2: outside → pending enter
      if (!state.pendingEnterAt) {
        state.pendingEnterAt = now;
      } else if (now - state.pendingEnterAt >= DEBOUNCE_MS) {
        // Stage 3: debounce passed → check cooldowns
        const inShortCooldown = state.shortCooldownUntil && now < state.shortCooldownUntil;
        const inLongCooldown = state.longCooldownUntil && now < state.longCooldownUntil;
        if (!inShortCooldown && !inLongCooldown) {
          candidates.push({ poi, distance: dist });
        }
      }
    } else if (state.isInsideZone && dist >= exitThreshold) {
      // Stage 7: exit
      state.isInsideZone = false;
      state.pendingEnterAt = undefined;
      state.lastExitAt = now;
      self.postMessage({ type: "EXIT", poiId: poi.id });
    } else if (state.isInsideZone || dist > enterThreshold) {
      // Still inside or moved away cleanly
      state.pendingEnterAt = undefined;
    }
  }

  // Stage 5: pick best candidate
  if (candidates.length === 0) return;
  candidates.sort((a, b) =>
    b.poi.priority - a.poi.priority || a.distance - b.distance
  );
  const best = candidates[0];
  const state = getState(best.poi.id);

  // Stage 6: trigger
  state.isInsideZone = true;
  state.pendingEnterAt = undefined;
  state.lastTriggeredAt = now;
  state.shortCooldownUntil = now + SHORT_COOLDOWN_MS;
  state.longCooldownUntil = now + LONG_COOLDOWN_MS;

  self.postMessage({ type: "TRIGGER", poiId: best.poi.id, distance: best.distance });
}

self.onmessage = (e: MessageEvent) => {
  const { type, payload } = e.data;

  if (type === "INIT") {
    pois = payload.pois ?? [];
    startWatching();
  } else if (type === "UPDATE_POIS") {
    pois = payload.pois ?? [];
  } else if (type === "STOP") {
    if (watchId !== null) self.clearInterval(watchId);
  }
};

function startWatching() {
  self.navigator.geolocation.watchPosition(
    (pos) => {
      self.postMessage({ type: "LOCATION", lat: pos.coords.latitude, lng: pos.coords.longitude });
      if (pos.coords.accuracy > MIN_ACCURACY_M) return;
      processLocation(pos.coords.latitude, pos.coords.longitude);
    },
    (err) => self.postMessage({ type: "GPS_ERROR", message: err.message }),
    { enableHighAccuracy: true, maximumAge: 5000, timeout: 10000 }
  );
}
