using AudioTravelling.Core.Interfaces;

namespace AudioTravelling.Infrastructure.Services;

public class OnlineTracker : IOnlineTracker
{
    private int _count;

    public void Increment() => Interlocked.Increment(ref _count);
    public void Decrement() => Interlocked.Decrement(ref _count);
    public int GetCount() => _count;
}
