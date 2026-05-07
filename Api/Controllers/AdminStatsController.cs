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
        public async Task<IActionResult> GetSessions([FromQuery] string period = "today", [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var now = DateTime.UtcNow;
            var startDate = now.Date;

            if (period == "week") startDate = now.AddDays(-7).Date;
            else if (period == "month") startDate = now.AddMonths(-1).Date;
            else if (period == "all") startDate = DateTime.MinValue;

            // If explicit dates are provided, they override the period
            if (fromDate.HasValue) startDate = fromDate.Value.ToUniversalTime();
            var endDate = toDate.HasValue ? toDate.Value.ToUniversalTime().AddDays(1).AddTicks(-1) : DateTime.MaxValue;

            var sessionsQuery = _context.AccessSessions
                .Include(s => s.QrCode)
                .Where(s => s.IssuedAt >= startDate && s.IssuedAt <= endDate);

            var sessions = await sessionsQuery.ToListAsync();

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

        [HttpPost("test-session")]
        public async Task<IActionResult> CreateTestSession()
        {
            // Find a random active QR code or create one
            var qr = await _context.QrCodes.FirstOrDefaultAsync(q => q.IsActive);
            if (qr == null)
            {
                qr = new QrCode
                {
                    QrCodeValue = "QR-TEST-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.QrCodes.Add(qr);
                await _context.SaveChangesAsync();
            }

            var session = new AccessSession
            {
                QrCodeID = qr.QrCodeID,
                DeviceID = "test-device-" + Guid.NewGuid().ToString("N").Substring(0, 4),
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(30),
                IsRevoked = false
            };

            _context.AccessSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test session created successfully", session });
        }
    }
}
