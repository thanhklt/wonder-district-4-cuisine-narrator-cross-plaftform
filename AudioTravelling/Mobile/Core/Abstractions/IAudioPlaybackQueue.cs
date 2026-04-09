namespace AudioTravelling.Mobile.Core.Abstractions;

/// <summary>
/// Manages a queue of audio tracks for sequential playback.
/// </summary>
public interface IAudioPlaybackQueue
{
    /// <summary>Enqueue an audio track by file path.</summary>
    void Enqueue(string filePath);

    /// <summary>Dequeue and return the next audio track path, or null if empty.</summary>
    string? Dequeue();

    /// <summary>Clear all queued tracks.</summary>
    void Clear();

    /// <summary>The number of tracks currently in the queue.</summary>
    int Count { get; }
}
