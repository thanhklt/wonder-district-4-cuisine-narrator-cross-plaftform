using SQLite;
using Shared.Models;

namespace Mobile.Services
{
    public class AudioService
    {
        private SQLiteAsyncConnection? _database;

        async Task Init()
        {
            if (_database != null) return;

            // Tạo đường dẫn file DB trên Android
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "AudioTravel.db3");

            _database = new SQLiteAsyncConnection(dbPath);

            // Tạo bảng nếu chưa tồn tại
            await _database.CreateTableAsync<LocalAudio>();
        }

        // Hàm kiểm tra audio dành riêng cho đồ án của anh
        public async Task<bool> CheckAudioExists(string langCode)
        {
            await Init();
            var item = await _database.Table<LocalAudio>()
                                       .FirstOrDefaultAsync(x => x.LanguageCode == langCode);

            // Trả về true nếu bản ghi tồn tại và file vật lý vẫn còn trong máy
            return item != null && File.Exists(item.FilePath);
        }

        // Hàm lưu thông tin sau khi tải xong
        public async Task SaveAudioInfo(LocalAudio audio)
        {
            await Init();
            await _database.InsertOrReplaceAsync(audio);
        }

        // Hàm này lấy thông tin audio từ SQLite dựa vào mã ngôn ngữ
        public async Task<LocalAudio> GetAudioByLang(string langCode)
        {
            await Init(); // Luôn đảm bảo DB đã được khởi tạo trước khi truy vấn

            // Tìm kiếm bản ghi có LanguageCode khớp với tham số truyền vào
            return await _database.Table<LocalAudio>()
                                  .FirstOrDefaultAsync(x => x.LanguageCode == langCode);
        }
    }
}
