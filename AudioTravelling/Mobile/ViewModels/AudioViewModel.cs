using Mobile.Services;
using MvvmHelpers;
using Plugin.Maui.Audio;
using Shared.Models;

namespace Mobile.ViewModels
{
    public class AudioViewModel : BaseViewModel
    {
        private readonly AudioService _audioService;
        private readonly IAudioManager _audioManager; // Thêm cái này
        private IAudioPlayer _activePlayer;           // Lưu trình phát hiện tại

        public AudioViewModel(AudioService _audioService, IAudioManager audioManager)
        {
            this._audioService = _audioService;
            _audioManager = audioManager;
        }

        public async Task HandleAudioSelection(string langCode, string downloadUrl)
        {
            // 1. Kiểm tra xem máy đã có audio chưa
            bool isExists = await _audioService.CheckAudioExists(langCode);

            if (isExists)
            {
                // 2. Nếu có rồi -> Phát nhạc
                PlayLocalAudio(langCode);
            }
            else
            {
                // 3. Nếu chưa có -> Thông báo và tải về
                bool userConsent = await Shell.Current.DisplayAlertAsync("Thông báo", "Bạn có muốn tải audio ngôn ngữ này?", "Có", "Không");
                if (userConsent)
                {
                    await DownloadAndSaveAudio(langCode, downloadUrl);
                }
            }
        }

        private async Task DownloadAndSaveAudio(string langCode, string url)
        {
            using var client = new HttpClient();
            var data = await client.GetByteArrayAsync(url);

            // Tạo tên file và đường dẫn lưu trữ trên Android
            string fileName = $"audio_{langCode}.mp3";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // Ghi file vào bộ nhớ máy
            await File.WriteAllBytesAsync(filePath, data);

            // Lưu thông tin vào SQLite để lần sau không phải tải lại
            var newAudio = new LocalAudio
            {
                LanguageCode = langCode,
                FilePath = filePath
            };
            await _audioService.SaveAudioInfo(newAudio);

            await Shell.Current.DisplayAlertAsync("Thành công", "Đã tải xong audio!", "OK");
        }

        public async Task PlayLocalAudio(string langCode)
        {
            // 1. Lấy thông tin từ SQLite
            var audioInfo = await _audioService.GetAudioByLang(langCode);

            if (audioInfo != null && File.Exists(audioInfo.FilePath))
            {
                // 2. Mở file audio từ đường dẫn đã lưu
                var audioFile = File.OpenRead(audioInfo.FilePath);

                // 3. Tạo trình phát và chạy
                _activePlayer = _audioManager.CreatePlayer(audioFile);
                _activePlayer.Play();

                if (_activePlayer != null && _activePlayer.IsPlaying)
                {
                    _activePlayer.Stop();
                }
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Lỗi", "Không tìm thấy file audio!", "OK");
            }
        }
    }
}
