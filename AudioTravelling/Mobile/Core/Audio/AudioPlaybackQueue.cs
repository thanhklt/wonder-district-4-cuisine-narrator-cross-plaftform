using AudioTravelling.Mobile.Core.Abstractions;

namespace AudioTravelling.Mobile.Core.Audio;

/// <summary>
/// FIFO queue for audio tracks to be played sequentially.
/// </summary>
public class AudioPlaybackQueue : IAudioPlaybackQueue
{
    private readonly Queue<string> _queue = new();

    public int Count => _queue.Count;

    public void Enqueue(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        _queue.Enqueue(filePath);
    }

    public string? Dequeue()
    {
        return _queue.Count > 0 ? _queue.Dequeue() : null;
    }

    public void Clear()
    {
        _queue.Clear();
    }
}
