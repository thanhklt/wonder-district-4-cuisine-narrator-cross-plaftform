using SQLite;
using AudioTravelling.Mobile.Features.Audio.Models;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public class AudioService
{
    private SQLiteAsyncConnection? _database;

    async Task Init()
    {
        if (_database != null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "AudioTravel.db3");

        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<LocalAudio>();
    }

    public async Task<bool> CheckAudioExists(string langCode)
    {
        await Init();
        var item = await _database.Table<LocalAudio>()
                                   .FirstOrDefaultAsync(x => x.LanguageCode == langCode);

        return item != null && File.Exists(item.FilePath);
    }

    public async Task SaveAudioInfo(LocalAudio audio)
    {
        await Init();
        await _database.InsertOrReplaceAsync(audio);
    }

    public async Task<LocalAudio> GetAudioByLang(string langCode)
    {
        await Init();

        return await _database.Table<LocalAudio>()
                              .FirstOrDefaultAsync(x => x.LanguageCode == langCode);
    }
}
