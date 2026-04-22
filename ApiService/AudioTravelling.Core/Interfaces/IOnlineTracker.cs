namespace AudioTravelling.Core.Interfaces;

public interface IOnlineTracker
{
    void Increment();
    void Decrement();
    int GetCount();
}
