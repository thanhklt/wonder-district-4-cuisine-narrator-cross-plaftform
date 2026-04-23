using AudioTravelling.API.Hubs;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Services;

public class ActiveSessionBroadcaster(
    IServiceScopeFactory scopeFactory,
    IHubContext<AdminHub> hubContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            var count = await db.AccessSessions
                .CountAsync(s => s.ExpiresAt > DateTime.UtcNow, stoppingToken);

            await hubContext.Clients.All.SendAsync("OnlineCount", count, stoppingToken);
        }
    }
}
