using AudioTravelling.Mobile.Features.Audio.Models;
using AudioTravelling.Mobile.Features.Audio.Services;
using MvvmHelpers;
using Plugin.Maui.Audio;
using System.Windows.Input;

namespace AudioTravelling.Mobile.Features.Audio.ViewModels;

public class AudioViewModel : BaseViewModel
{
    private readonly AudioService _audioService;
    private readonly IAudioManager _audioManager;
    private readonly ITextToSpeechService _ttsService;
    private IAudioPlayer _activePlayer;

    public ICommand SpeakDescriptionCommand { get; }
    public ICommand StopSpeakingCommand { get; }

    public AudioViewModel(
        AudioService audioService,
        IAudioManager audioManager,
        ITextToSpeechService ttsService)
    {
        _audioService = audioService;
        _audioManager = audioManager;
        _ttsService = ttsService;

        SpeakDescriptionCommand = new Command<string>(async (text) => await SpeakDescriptionAsync(text));
        StopSpeakingCommand = new Command(() => StopSpeaking());
    }

    // ── Text-to-Speech ───────────────────────────────────────────────

    /// <summary>
    /// Speaks a POI description aloud using the device's TTS engine.
    /// </summary>
    /// <param name="text">The description text to read.</param>
    /// <param name="languageCode">Optional BCP-47 language code (e.g., "vi-VN").</param>
    public async Task SpeakDescriptionAsync(string text, string languageCode = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await Shell.Current.DisplayAlertAsync("Thông báo", "Không có nội dung để đọc.", "OK");
            return;
        }

        try
        {
            await _ttsService.SpeakAsync(text, languageCode);
        }
        catch (OperationCanceledException)
        {
            // Playback was cancelled – this is expected, no action needed.
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Lỗi", $"Không thể phát giọng đọc: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Stops any active TTS playback.
    /// </summary>
    public void StopSpeaking()
    {
        _ttsService.CancelPlayback();
    }

    // ── Audio File Playback ──────────────────────────────────────────

    public async Task HandleAudioSelection(string langCode, string downloadUrl)
    {
        bool isExists = await _audioService.CheckAudioExists(langCode);

        if (isExists)
        {
            PlayLocalAudio(langCode);
        }
        else
        {
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

        string fileName = $"audio_{langCode}.mp3";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, data);

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
        var audioInfo = await _audioService.GetAudioByLang(langCode);

        if (audioInfo != null && File.Exists(audioInfo.FilePath))
        {
            var audioFile = File.OpenRead(audioInfo.FilePath);

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

