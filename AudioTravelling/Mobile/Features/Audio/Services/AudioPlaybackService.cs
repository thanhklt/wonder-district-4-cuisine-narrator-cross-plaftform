using Plugin.Maui.Audio;

namespace AudioTravelling.Mobile.Features.Audio.Services;

/// <summary>
/// Manages audio playback for the app using Plugin.Maui.Audio.
/// Provides Play, Pause, Resume, Stop, Speed control, and progress tracking.
/// Handles errors gracefully when audio files are missing or unreadable.
/// </summary>
public sealed class AudioPlaybackService : IDisposable
{
    // ── Singleton ──────────────────────────────────────────────────────
    private static AudioPlaybackService? _instance;
    public static AudioPlaybackService Instance => _instance ??= new AudioPlaybackService();

    // ── Fields ─────────────────────────────────────────────────────────
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;
    private IDispatcherTimer? _progressTimer;
    private bool _disposed;

    // ── State properties ───────────────────────────────────────────────
    public bool   IsPlaying  => _player?.IsPlaying ?? false;
    public bool   IsPaused   { get; private set; }
    public bool   IsStopped  => !IsPlaying && !IsPaused;
    public double Duration   => _player?.Duration ?? 0;
    public double Position   => _player?.CurrentPosition ?? 0;
    public double Progress   => Duration > 0 ? Position / Duration : 0;
    public string CurrentFile { get; private set; } = string.Empty;

    // ── Events ─────────────────────────────────────────────────────────
    public event Action<double, double>? ProgressChanged;
    public event Action<PlaybackState>? StateChanged;
    public event Action? PlaybackEnded;
    public event Action<string>? ErrorOccurred;

    // ── Constructor ────────────────────────────────────────────────────
    private AudioPlaybackService()
    {
        _audioManager = AudioManager.Current;
    }

    // ══════════════════════════════════════════════════════════════════
    //  PLAY
    // ══════════════════════════════════════════════════════════════════
    public async Task PlayAsync(string fileNameOrPath)
    {
        try
        {
            await StopInternalAsync();

            Stream? audioStream = null;

            if (Path.IsPathRooted(fileNameOrPath))
            {
                if (!File.Exists(fileNameOrPath))
                {
                    RaiseError($"Không tìm thấy file audio: {fileNameOrPath}");
                    return;
                }
                audioStream = File.OpenRead(fileNameOrPath);
            }
            else
            {
                try
                {
                    audioStream = await FileSystem.OpenAppPackageFileAsync(fileNameOrPath);
                }
                catch (FileNotFoundException)
                {
                    RaiseError($"File audio '{fileNameOrPath}' không có trong Resources/Raw.");
                    return;
                }
                catch (Exception ex)
                {
                    RaiseError($"Không thể mở file audio '{fileNameOrPath}': {ex.Message}");
                    return;
                }
            }

            if (audioStream is null)
            {
                RaiseError("Không thể đọc luồng audio.");
                return;
            }

            _player = _audioManager.CreatePlayer(audioStream);
            _player.PlaybackEnded += OnPlaybackEnded;

            CurrentFile = fileNameOrPath;
            IsPaused = false;

            _player.Play();
            StartProgressTimer();
            RaiseStateChanged(PlaybackState.Playing);
        }
        catch (Exception ex)
        {
            RaiseError($"Lỗi khi phát audio: {ex.Message}");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  PAUSE
    // ══════════════════════════════════════════════════════════════════
    public void Pause()
    {
        if (_player is null || !_player.IsPlaying) return;

        _player.Pause();
        IsPaused = true;
        StopProgressTimer();
        RaiseStateChanged(PlaybackState.Paused);
    }

    // ══════════════════════════════════════════════════════════════════
    //  RESUME
    // ══════════════════════════════════════════════════════════════════
    public void Resume()
    {
        if (_player is null || !IsPaused) return;

        _player.Play();
        IsPaused = false;
        StartProgressTimer();
        RaiseStateChanged(PlaybackState.Playing);
    }

    // ══════════════════════════════════════════════════════════════════
    //  STOP
    // ══════════════════════════════════════════════════════════════════
    public async Task StopAsync()
    {
        await StopInternalAsync();
        RaiseStateChanged(PlaybackState.Stopped);
    }

    // ══════════════════════════════════════════════════════════════════
    //  TOGGLE
    // ══════════════════════════════════════════════════════════════════
    public async Task TogglePlayPauseAsync(string? fileNameOrPath = null)
    {
        if (IsPlaying)
        {
            Pause();
        }
        else if (IsPaused)
        {
            Resume();
        }
        else if (!string.IsNullOrEmpty(fileNameOrPath))
        {
            await PlayAsync(fileNameOrPath);
        }
        else if (!string.IsNullOrEmpty(CurrentFile))
        {
            await PlayAsync(CurrentFile);
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  SPEED
    // ══════════════════════════════════════════════════════════════════
    public void SetSpeed(double speed)
    {
        if (_player is null) return;
        speed = Math.Clamp(speed, 0.25, 3.0);
        _player.Speed = speed;
    }

    // ══════════════════════════════════════════════════════════════════
    //  SEEK
    // ══════════════════════════════════════════════════════════════════
    public void Seek(double positionSeconds)
    {
        if (_player is null) return;
        _player.Seek(positionSeconds);
    }

    public static string FormatTime(double totalSeconds)
    {
        if (totalSeconds < 0 || double.IsNaN(totalSeconds) || double.IsInfinity(totalSeconds))
            return "0:00";

        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.Hours > 0
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }

    // ══════════════════════════════════════════════════════════════════
    //  INTERNALS
    // ══════════════════════════════════════════════════════════════════
    private Task StopInternalAsync()
    {
        StopProgressTimer();

        if (_player is not null)
        {
            if (_player.IsPlaying)
                _player.Stop();

            _player.PlaybackEnded -= OnPlaybackEnded;
            _player.Dispose();
            _player = null;
        }

        IsPaused = false;
        return Task.CompletedTask;
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopProgressTimer();
            IsPaused = false;
            RaiseStateChanged(PlaybackState.Stopped);
            PlaybackEnded?.Invoke();
        });
    }

    private void StartProgressTimer()
    {
        StopProgressTimer();

        _progressTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_progressTimer is null) return;

        _progressTimer.Interval = TimeSpan.FromMilliseconds(250);
        _progressTimer.Tick += OnProgressTimerTick;
        _progressTimer.Start();
    }

    private void StopProgressTimer()
    {
        if (_progressTimer is null) return;
        _progressTimer.Stop();
        _progressTimer.Tick -= OnProgressTimerTick;
        _progressTimer = null;
    }

    private void OnProgressTimerTick(object? sender, EventArgs e)
    {
        if (_player is null) return;
        ProgressChanged?.Invoke(_player.CurrentPosition, _player.Duration);
    }

    private void RaiseStateChanged(PlaybackState state) =>
        StateChanged?.Invoke(state);

    private void RaiseError(string message) =>
        ErrorOccurred?.Invoke(message);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopProgressTimer();
        if (_player is not null)
        {
            if (_player.IsPlaying) _player.Stop();
            _player.PlaybackEnded -= OnPlaybackEnded;
            _player.Dispose();
            _player = null;
        }
    }
}

// ── Playback state enum ────────────────────────────────────────────────
public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}
