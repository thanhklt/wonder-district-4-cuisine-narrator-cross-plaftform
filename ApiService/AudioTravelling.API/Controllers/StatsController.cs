using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(Roles = "Admin")]
public class StatsController(IAppDbContext db, IOnlineTracker tracker) : ControllerBase
{
    [HttpGet("realtime")]
    public IActionResult GetRealtime() => Ok(new { onlineCount = tracker.GetCount() });

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] string period = "day")
    {
        var (from, groupBy) = ParsePeriod(period);

        var sessions = await db.AccessSessions
            .Where(s => s.CreatedAt >= from)
            .ToListAsync();

        var grouped = groupBy switch
        {
            "hour" => sessions.GroupBy(s => s.CreatedAt.ToString("yyyy-MM-dd HH:00")),
            "day" => sessions.GroupBy(s => s.CreatedAt.ToString("yyyy-MM-dd")),
            "month" => sessions.GroupBy(s => s.CreatedAt.ToString("yyyy-MM")),
            _ => sessions.GroupBy(s => s.CreatedAt.ToString("yyyy-MM-dd")),
        };

        return Ok(grouped.Select(g => new { date = g.Key, count = g.Count() }).OrderBy(x => x.date));
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatmap([FromQuery] string period = "day")
    {
        var (from, _) = ParsePeriod(period);
        var points = await db.AccessSessions
            .Where(s => s.CreatedAt >= from && s.Lat != null && s.Lng != null)
            .Select(s => new { lat = s.Lat, lng = s.Lng })
            .ToListAsync();
        return Ok(points);
    }

    private static (DateTime from, string groupBy) ParsePeriod(string period) => period switch
    {
        "day" => (DateTime.UtcNow.AddDays(-1), "hour"),
        "3days" => (DateTime.UtcNow.AddDays(-3), "day"),
        "week" => (DateTime.UtcNow.AddDays(-7), "day"),
        "month" => (DateTime.UtcNow.AddMonths(-1), "day"),
        "year" => (DateTime.UtcNow.AddYears(-1), "month"),
        _ => (DateTime.UtcNow.AddDays(-1), "hour"),
    };
}
