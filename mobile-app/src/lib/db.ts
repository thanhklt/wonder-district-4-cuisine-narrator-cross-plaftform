import Dexie, { type EntityTable } from "dexie";

export interface CachedPoi {
  id: string;
  name: string;
  lat: number;
  lng: number;
  radiusMeters: number;
  priority: number;
}

export interface CachedPoiImage {
  id: string;
  poiId: string;
  imageUrl: string;
  order: number;
}

export interface CachedPoiLocalization {
  id: string;
  poiId: string;
  language: string;
  textContent: string;
  audioUrl?: string;
}

export interface CachedPoiAudio {
  id: string; // `${poiId}-${language}`
  poiId: string;
  language: string;
  audioBlob: Blob;
}

export interface PoiGeofenceState {
  poiId: string;
  isInsideZone: boolean;
  pendingEnterAt?: number;
  lastTriggeredAt?: number;
  shortCooldownUntil?: number;
  longCooldownUntil?: number;
  lastExitAt?: number;
}

export interface AudioPlaybackHistory {
  id: string;
  poiId: string;
  language: string;
  playedAt: number;
}

class AppDB extends Dexie {
  pois!: EntityTable<CachedPoi, "id">;
  poiImages!: EntityTable<CachedPoiImage, "id">;
  poiLocalizations!: EntityTable<CachedPoiLocalization, "id">;
  poiAudios!: EntityTable<CachedPoiAudio, "id">;
  geofenceState!: EntityTable<PoiGeofenceState, "poiId">;
  playbackHistory!: EntityTable<AudioPlaybackHistory, "id">;

  constructor() {
    super("AudioTravelling");
    this.version(1).stores({
      pois: "id, name",
      poiImages: "id, poiId",
      poiLocalizations: "id, poiId, language, [poiId+language]",
      poiAudios: "id, poiId, language",
      geofenceState: "poiId",
      playbackHistory: "id, poiId, playedAt",
    });
  }
}

export const db = new AppDB();
