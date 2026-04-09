using SQLite;
using AudioTravelling.Mobile.Features.Audio.Models;

namespace AudioTravelling.Mobile.Data.SQLite;

public class AppDbContext
{
    private SQLiteAsyncConnection _database;

    public AppDbContext(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        InitializeTables().Wait();
    }

    private async Task InitializeTables()
    {
        await _database.CreateTableAsync<LocalAudio>();
    }

    // ── LocalAudio CRUD ───────────────────────────────────────────────
    public async Task<List<LocalAudio>> GetAllAudiosAsync()
    {
        return await _database.Table<LocalAudio>().ToListAsync();
    }

    public async Task<int> SaveAudioAsync(LocalAudio audio)
    {
        return await _database.InsertOrReplaceAsync(audio);
    }
}
