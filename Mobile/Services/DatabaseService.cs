using SQLite;
using Mobile.Models;

namespace Mobile.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private async Task InitAsync()
    {
        if (_db is not null) return;
        await _initLock.WaitAsync();
        try
        {
            if (_db is not null) return; // double-check sau khi co lock
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "audiotravelling.db");
            var conn = new SQLiteAsyncConnection(dbPath);
            await conn.CreateTableAsync<CachedPoi>();
            await conn.CreateTableAsync<CachedPoiLocalization>();
            await conn.CreateTableAsync<CachedPoiAudio>();
            await conn.CreateTableAsync<CachedAccessSession>();
            await conn.CreateTableAsync<PoiGeofenceState>();
            await conn.CreateTableAsync<AudioPlaybackHistory>();
            _db = conn; // chi gan sau khi tat ca bang da duoc tao xong
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ── Session ──────────────────────────────────────────────
    public async Task<CachedAccessSession?> GetValidSessionAsync(string deviceId)
    {
        await InitAsync();
        return await _db!.Table<CachedAccessSession>()
            .Where(s => s.DeviceID == deviceId && !s.IsRevoked)
            .OrderByDescending(s => s.IssuedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveSessionAsync(CachedAccessSession session)
    {
        await InitAsync();
        await _db!.InsertOrReplaceAsync(session);
    }

    public async Task InvalidateSessionAsync(string deviceId)
    {
        await InitAsync();
        var sessions = await _db!.Table<CachedAccessSession>()
            .Where(s => s.DeviceID == deviceId).ToListAsync();
        foreach (var s in sessions)
        {
            s.IsRevoked = true;
            await _db.UpdateAsync(s);
        }
    }

    // ── POIs ─────────────────────────────────────────────────
    public async Task<List<CachedPoi>> GetActivePoisAsync()
    {
        await InitAsync();
        return await _db!.Table<CachedPoi>().Where(p => p.IsActive).ToListAsync();
    }

    public async Task UpsertPoisAsync(IEnumerable<CachedPoi> pois)
    {
        await InitAsync();
        foreach (var poi in pois)
            await _db!.InsertOrReplaceAsync(poi);
    }

    public async Task DeactivateAllPoisAsync()
    {
        await InitAsync();
        await _db!.ExecuteAsync("UPDATE CachedPois SET IsActive = 0");
    }

    // ── Localizations ─────────────────────────────────────────
    public async Task<CachedPoiLocalization?> GetLocalizationAsync(int poiId, string langCode)
    {
        await InitAsync();
        return await _db!.Table<CachedPoiLocalization>()
            .Where(l => l.PoiID == poiId && l.LanguageCode == langCode)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertLocalizationsAsync(IEnumerable<CachedPoiLocalization> localizations)
    {
        await InitAsync();
        foreach (var l in localizations)
            await _db!.InsertOrReplaceAsync(l);
    }

    // Luu 1 localization theo (PoiID, LanguageCode) — dung sau khi TTS proxy tao text moi
    public async Task SaveLocalizationAsync(CachedPoiLocalization loc)
    {
        await InitAsync();
        // Xoa ban ghi cu (neu co) truoc khi insert moi, vi khoa chinh la LocalizationID
        await _db!.ExecuteAsync(
            "DELETE FROM CachedPoiLocalizations WHERE PoiID = ? AND LanguageCode = ?",
            loc.PoiID, loc.LanguageCode);
        await _db.InsertAsync(loc);
    }

    // ── Audio cache ───────────────────────────────────────────
    public async Task<CachedPoiAudio?> GetAudioAsync(int poiId, string langCode)
    {
        await InitAsync();
        return await _db!.Table<CachedPoiAudio>()
            .Where(a => a.PoiID == poiId && a.LanguageCode == langCode)
            .FirstOrDefaultAsync();
    }

    public async Task SaveAudioAsync(CachedPoiAudio audio)
    {
        await InitAsync();
        await _db!.InsertOrReplaceAsync(audio);
    }

    // ── Geofence state ────────────────────────────────────────
    public async Task<PoiGeofenceState?> GetGeofenceStateAsync(int poiId)
    {
        await InitAsync();
        return await _db!.Table<PoiGeofenceState>()
            .Where(g => g.PoiID == poiId).FirstOrDefaultAsync();
    }

    public async Task SaveGeofenceStateAsync(PoiGeofenceState state)
    {
        await InitAsync();
        state.UpdatedAt = DateTime.UtcNow;
        await _db!.InsertOrReplaceAsync(state);
    }

    // ── Audio history ─────────────────────────────────────────
    public async Task LogPlaybackAsync(AudioPlaybackHistory history)
    {
        await InitAsync();
        await _db!.InsertAsync(history);
    }
}
