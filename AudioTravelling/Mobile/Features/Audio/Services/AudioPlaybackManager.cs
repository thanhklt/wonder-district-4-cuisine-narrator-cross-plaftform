using System.Globalization;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public class AudioPlaybackManager : IAudioPlaybackManager
{
    private readonly IAudioCacheService _audioCacheService;
    private readonly AudioPlaybackService _playerService;
    
    // Playback state
    private Models.AudioPlayRequest? _currentPlayingItem;
    private Models.AudioPlayRequest? _pendingItem;
    private bool _isProcessingQueue;

    public AudioPlaybackManager(IAudioCacheService audioCacheService)
    {
        _audioCacheService = audioCacheService;
        _playerService = AudioPlaybackService.Instance;
        _playerService.PlaybackEnded += OnPlaybackEnded;
        _playerService.ErrorOccurred += OnPlaybackError;
    }

    public int? CurrentPlayingPoiId => _currentPlayingItem?.PoiId;

    public async Task RequestPlayAsync(Models.AudioPlayRequest request)
    {
        if (string.IsNullOrEmpty(request.LanguageCode))
        {
            request.LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower() == "vi" ? "vi" : "en";
        }

        if (request.IsManualClick)
        {
            // Clear queue and stop current playback immediately
            _pendingItem = null;
            await _playerService.StopAsync();
            _currentPlayingItem = null;
            
            // Set as current and play immediately
            _currentPlayingItem = request;
            _ = ProcessPlayItemAsync(request);
            return;
        }

        // --- Geofence Logic ---
        
        // Anti-spam: if it's already playing, ignore
        if (_currentPlayingItem != null && _currentPlayingItem.PoiId == request.PoiId)
        {
            return;
        }

        // Anti-spam: if it's already in queue, ignore
        if (_pendingItem != null && _pendingItem.PoiId == request.PoiId)
        {
            return;
        }

        if (_currentPlayingItem == null && !_playerService.IsPlaying)
        {
            // Nothing is playing, play immediately
            _currentPlayingItem = request;
            _ = ProcessPlayItemAsync(request);
        }
        else
        {
            // Something is playing, queue it (max 1 pending item)
            // Replace existing pending item if any
            _pendingItem = request;
        }
    }

    public void StopAndClear()
    {
        _pendingItem = null;
        _ = _playerService.StopAsync();
        _currentPlayingItem = null;
    }

    private async Task ProcessPlayItemAsync(Models.AudioPlayRequest request)
    {
        if (_isProcessingQueue) return;
        _isProcessingQueue = true;

        try
        {
            // Fetch/Download audio
            var localPath = await _audioCacheService.GetOrDownloadAudioAsync(request.PoiId, request.LanguageCode);

            // Playback
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                // Double check if it got preempted while downloading
                if (_currentPlayingItem == request) 
                {
                    await _playerService.PlayAsync(localPath);
                }
            }
            else
            {
                // Failed to get audio, move to next
                MoveToNextInQueue();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing audio for POI {request.PoiId}: {ex.Message}");
            MoveToNextInQueue();
        }
        finally
        {
            _isProcessingQueue = false;
        }
    }

    private void OnPlaybackEnded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MoveToNextInQueue();
        });
    }

    private void OnPlaybackError(string errorMessage)
    {
        System.Diagnostics.Debug.WriteLine($"Playback error: {errorMessage}");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MoveToNextInQueue();
        });
    }

    private void MoveToNextInQueue()
    {
        if (_pendingItem != null)
        {
            var nextItem = _pendingItem;
            _pendingItem = null;
            _currentPlayingItem = nextItem;
            _ = ProcessPlayItemAsync(nextItem);
        }
        else
        {
            _currentPlayingItem = null;
        }
    }
}
