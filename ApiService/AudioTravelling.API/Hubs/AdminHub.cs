using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AudioTravelling.API.Hubs;

public class AdminHub(IOnlineTracker tracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        tracker.Increment();
        await Clients.All.SendAsync("OnlineCount", tracker.GetCount());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        tracker.Decrement();
        await Clients.All.SendAsync("OnlineCount", tracker.GetCount());
        await base.OnDisconnectedAsync(exception);
    }
}
