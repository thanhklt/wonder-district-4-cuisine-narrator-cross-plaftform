const DEBOUNCE_MS = 3000;
const BUFFER_M = 1;
const SHORT_COOLDOWN_MS = 45_000;
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
  lastExitAt?: number;
}

interface PersistedGeofenceState extends GeofenceState {
  poiId: string;
}

interface LocationPayload {
  lat: number;
  lng: number;
  accuracy: number;
  recordedAt?: number;
}

let pois: Poi[] = [];
const states = new Map<string, GeofenceState>();
let latestLocation: LocationPayload | null = null;
let pendingTimer: ReturnType<typeof setTimeout> | null = null;

// Tinh khoang cach user-POI bang cong thuc Haversine.
function toRad(deg: number) {
  return (deg * Math.PI) / 180;
}

function distanceMeters(lat1: number, lng1: number, lat2: number, lng2: number): number {
  const earthRadius = 6371000;
  const dLat = toRad(lat2 - lat1);
  const dLng = toRad(lng2 - lng1);
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng / 2) ** 2;

  return earthRadius * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

// Moi POI co mot state rieng de theo doi outside/inside/pending enter/cooldown.
function getState(poiId: string): GeofenceState {
  if (!states.has(poiId)) {
    states.set(poiId, { isInsideZone: false });
  }
  return states.get(poiId)!;
}

// Dong bo state ve main thread de luu IndexedDB, giu duoc cooldown va lich su zone.
function emitState(poiId: string) {
  const state = getState(poiId);
  const payload: PersistedGeofenceState = {
    poiId,
    isInsideZone: state.isInsideZone,
    pendingEnterAt: state.pendingEnterAt,
    lastTriggeredAt: state.lastTriggeredAt,
    shortCooldownUntil: state.shortCooldownUntil,
    lastExitAt: state.lastExitAt,
  };

  self.postMessage({ type: "STATE_UPDATED", payload });
}

function getDistanceFromLatest(poi: Poi): number | null {
  if (!latestLocation) {
    return null;
  }

  return distanceMeters(latestLocation.lat, latestLocation.lng, poi.lat, poi.lng);
}

// Khi nhieu POI cung pending enter, uu tien POI priority cao hon.
// Neu cung priority thi chon POI gan user hon.
function sortByPriorityAndDistance(a: { poi: Poi; distance: number }, b: { poi: Poi; distance: number }) {
  return b.poi.priority - a.poi.priority || a.distance - b.distance;
}

// Debounce da pass: danh dau user da vao vung that su.
function markEntered(poiId: string, enteredAt: number) {
  const state = getState(poiId);
  state.isInsideZone = true;
  state.pendingEnterAt = undefined;
  emitState(poiId);
  self.postMessage({ type: "ENTER_CONFIRMED", poiId, enteredAt });
}

// Giai doan 3: chon pending enter "thang" trong priority queue va hen gio debounce 3s.
function scheduleTopPendingEnter() {
  if (pendingTimer) {
    clearTimeout(pendingTimer);
    pendingTimer = null;
  }

  if (!latestLocation) {
    return;
  }

  const pendingCandidates = pois // Priority queue: pending enter + uu tien priority cao + gan user.
    .map((poi) => {
      const state = getState(poi.id);
      const distance = getDistanceFromLatest(poi);
      if (state.isInsideZone || !state.pendingEnterAt || distance === null) {
        return null;
      }
      return { poi, state, distance };
    })
    .filter((item): item is { poi: Poi; state: GeofenceState; distance: number } => item !== null) 
    .sort(sortByPriorityAndDistance); // Sort để giả lập priorityqueue

  const nextCandidate = pendingCandidates[0];
  if (!nextCandidate) {
    return;
  }

  const dueAt = nextCandidate.state.pendingEnterAt! + DEBOUNCE_MS;
  const waitMs = Math.max(0, dueAt - Date.now());
  pendingTimer = setTimeout(() => confirmPendingEnter(nextCandidate.poi.id), waitMs);
}

// Sau 3s debounce, chi enter neu user van con nam trong nguong R - 1m.
// Neu qua cooldown 45s thi moi trigger audio va cap nhat LastTriggeredAt/CooldownUntil.
function confirmPendingEnter(poiId: string) {
  pendingTimer = null;

  const poi = pois.find((item) => item.id === poiId);
  if (!poi || !latestLocation) {
    scheduleTopPendingEnter();
    return;
  }

  const state = getState(poiId);
  const now = Date.now();
  const distance = distanceMeters(latestLocation.lat, latestLocation.lng, poi.lat, poi.lng);
  const enterThreshold = poi.radiusMeters - BUFFER_M;

  if (!state.pendingEnterAt || distance > enterThreshold) {
    state.pendingEnterAt = undefined;
    emitState(poiId);
    scheduleTopPendingEnter();
    return;
  }

  markEntered(poiId, now);

  const inShortCooldown = Boolean(state.shortCooldownUntil && now < state.shortCooldownUntil);
  if (!inShortCooldown) {
    state.lastTriggeredAt = now;
    state.shortCooldownUntil = now + SHORT_COOLDOWN_MS;
    emitState(poiId);
    self.postMessage({ type: "TRIGGER", poiId, distance });
  }

  scheduleTopPendingEnter();
}

// Giai doan 1 + 2:
// - Nhan GPS moi
// - Tinh khoang cach den tung POI
// - Ap dung hysteresis voi enter = R - 1m, exit = R + 1m
// - Outside -> pending enter
// - Inside -> exit neu vuot nguong thoat
function processLocation(location: LocationPayload) {
  latestLocation = location;
  const now = Date.now();

  for (const poi of pois) {
    const distance = distanceMeters(location.lat, location.lng, poi.lat, poi.lng);
    const enterThreshold = poi.radiusMeters - BUFFER_M;
    const exitThreshold = poi.radiusMeters + BUFFER_M;
    const state = getState(poi.id);

    if (!state.isInsideZone && distance <= enterThreshold) {
      if (!state.pendingEnterAt) {
        state.pendingEnterAt = now;
        emitState(poi.id);
      }
      continue;
    }

    if (state.isInsideZone && distance >= exitThreshold) {
      state.isInsideZone = false;
      state.pendingEnterAt = undefined;
      state.lastExitAt = now;
      emitState(poi.id);
      self.postMessage({ type: "EXIT", poiId: poi.id });
      continue;
    }

    if (!state.isInsideZone && state.pendingEnterAt && distance > enterThreshold) {
      state.pendingEnterAt = undefined;
      emitState(poi.id);
    }
  }

  scheduleTopPendingEnter();
}

// Worker chi xu ly geofence.
// Main thread chiu trach nhiem lay GPS moi 5s va gui LOCATION vao day.
self.onmessage = (event: MessageEvent) => {
  const { type, payload } = event.data;

  if (type === "INIT") {
    // Khoi tao danh sach POI va nap lai geofence state da luu tu truoc.
    pois = payload.pois ?? [];
    states.clear();
    for (const savedState of payload.states ?? []) {
      states.set(savedState.poiId, {
        isInsideZone: savedState.isInsideZone,
        pendingEnterAt: savedState.pendingEnterAt,
        lastTriggeredAt: savedState.lastTriggeredAt,
        shortCooldownUntil: savedState.shortCooldownUntil,
        lastExitAt: savedState.lastExitAt,
      });
    }
    scheduleTopPendingEnter();
    return;
  }

  if (type === "UPDATE_POIS") {
    // Khi bootstrap/sync thay doi danh sach POI, worker cap nhat tap POI ngay.
    pois = payload.pois ?? [];
    scheduleTopPendingEnter();
    return;
  }

  if (type === "LOCATION") {
    // Accuracy qua thap thi bo qua de tranh trigger sai geofence.
    const { lat, lng, accuracy, recordedAt } = payload as LocationPayload;
    if (accuracy > MIN_ACCURACY_M) {
      self.postMessage({ type: "LOCATION_SKIPPED", reason: "LOW_ACCURACY", accuracy, recordedAt });
      return;
    }

    processLocation({ lat, lng, accuracy, recordedAt });
  }
};

