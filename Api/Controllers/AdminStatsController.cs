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
        public async Task<IActionResult> GetSessions(
            [FromQuery] string period = "today",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var now = DateTime.UtcNow;

            var query = _context.AccessSessions
                .Include(s => s.QrCode)
                .AsQueryable();

            if (fromDate.HasValue || toDate.HasValue)
            {
                // Explicit date range takes priority over period
                if (fromDate.HasValue)
                    query = query.Where(s => s.IssuedAt >= fromDate.Value.Date);
                if (toDate.HasValue)
                    query = query.Where(s => s.IssuedAt < toDate.Value.Date.AddDays(1));
            }
            else
            {
                // Fallback to period-based filtering
                var startDate = now.Date;
                if (period == "week") startDate = now.AddDays(-7).Date;
                else if (period == "month") startDate = now.AddMonths(-1).Date;
                else if (period == "all") startDate = DateTime.MinValue;

                query = query.Where(s => s.IssuedAt >= startDate);
            }

            var sessions = await query.ToListAsync();

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

        // GET /api/admin/stats/daily-scans?days=7
        // Trả về số lượng session được tạo mỗi ngày trong N ngày gần nhất
        [HttpGet("daily-scans")]
        public async Task<IActionResult> GetDailyScans([FromQuery] int days = 7)
        {
            if (days < 1 || days > 90) days = 7;

            var now = DateTime.UtcNow.Date;
            var startDate = now.AddDays(-(days - 1));

            var sessions = await _context.AccessSessions
                .Where(s => s.IssuedAt >= startDate)
                .Select(s => s.IssuedAt.Date)
                .ToListAsync();

            // Tạo danh sách đủ ngày (kể cả ngày không có session = 0)
            var result = Enumerable.Range(0, days).Select(i =>
            {
                var date = startDate.AddDays(i);
                return new
                {
                    date  = date.ToString("dd/MM"),
                    count = sessions.Count(s => s == date)
                };
            }).ToList();

            return Ok(result);
        }

        // GET /api/admin/stats/access-sessions?from=&to=&status=active|expired|all
        [HttpGet("access-sessions")]
        public async Task<IActionResult> GetAccessSessions(
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] string? status)
        {
            var now = DateTime.UtcNow;

            var query = _context.AccessSessions
                .Include(s => s.QrCode)
                .AsQueryable();

            if (DateTime.TryParse(from, out var fromDate))
                query = query.Where(s => s.IssuedAt >= fromDate.ToUniversalTime());

            if (DateTime.TryParse(to, out var toDate))
                query = query.Where(s => s.IssuedAt < toDate.AddDays(1).ToUniversalTime());

            if (status == "active")
                query = query.Where(s => s.ExpiredAt > now && !s.IsRevoked);
            else if (status == "expired")
                query = query.Where(s => s.ExpiredAt <= now || s.IsRevoked);

            var sessions = await query
                .OrderByDescending(s => s.IssuedAt)
                .Select(s => new
                {
                    sessionId  = s.SessionID,
                    deviceId   = s.DeviceID,
                    qrCodeName = s.QrCode != null ? s.QrCode.QrCodeValue : "—",
                    issuedAt   = s.IssuedAt,
                    expiredAt  = s.ExpiredAt,
                    isExpired  = s.ExpiredAt <= now || s.IsRevoked
                })
                .ToListAsync();

            return Ok(sessions);
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
