using Api.Models;
using Api.Models.Entities;
using Api.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/admin/stats")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminStatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions([FromQuery] string period = "today")
        {
            var now = DateTime.UtcNow;
            var startDate = now.Date;

            if (period == "week") startDate = now.AddDays(-7).Date;
            else if (period == "month") startDate = now.AddMonths(-1).Date;
            else if (period == "all") startDate = DateTime.MinValue;

            var sessions = await _context.AccessSessions
                .Include(s => s.QrCode)
                .Where(s => s.IssuedAt >= startDate)
                .ToListAsync();

            var activeUsers = sessions.Count(s => !s.IsRevoked && s.ExpiredAt > now);
            var uniqueUsers = sessions.Select(s => s.DeviceID).Distinct().Count();

            var recentSessions = sessions.OrderByDescending(s => s.IssuedAt).Take(50).Select(s => new RecentSessionDto
            {
                SessionId = s.SessionID,
                QrCodeId = s.QrCodeID,
                Code = s.QrCode?.QrCodeValue ?? "",
                DeviceId = s.DeviceID,
                IssuedAt = s.IssuedAt,
                ExpiredAt = s.ExpiredAt,
                IsRevoked = s.IsRevoked
            }).ToList();

            return Ok(new
            {
                totalSessions = sessions.Count,
                totalScans = sessions.Count, // Same for now
                activeUsers = activeUsers,
                uniqueUsers = uniqueUsers,
                recentSessions = recentSessions
            });
        }

        [HttpGet("heatmap")]
        public IActionResult GetHeatmap([FromQuery] string period = "today")
        {
            // DB schema currently does not have Latitude/Longitude for AccessSessions.
            // TODO: Add Latitude/Longitude columns to AccessSessions or create AccessLogs table for real heatmap data.
            return Ok(new object[] { });
        }

        [HttpGet("realtime")]
        public async Task<IActionResult> GetRealtime()
        {
            var now = DateTime.UtcNow;
            var activeCount = await _context.AccessSessions
                .Where(s => !s.IsRevoked && s.ExpiredAt > now)
                .CountAsync();

            return Ok(new
            {
                activeCount = activeCount,
                lastUpdatedAt = now
            });
        }
    }
}
