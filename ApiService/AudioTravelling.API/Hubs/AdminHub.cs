using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Hubs;

public class AdminHub(IServiceScopeFactory scopeFactory) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var count = await GetActiveSessionCount();
        await Clients.Caller.SendAsync("OnlineCount", count);
        await base.OnConnectedAsync();
    }

    private async Task<int> GetActiveSessionCount()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        return await db.AccessSessions.CountAsync(s => s.ExpiresAt > DateTime.UtcNow);
    }
}
