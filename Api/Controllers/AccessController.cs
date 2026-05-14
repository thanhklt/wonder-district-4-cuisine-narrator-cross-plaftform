using Api.Models;
using Api.Models.Entities;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/access")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccessController(AppDbContext context)
        {
            _context = context;
        }

        // Validate DeviceProfile: chỉ chấp nhận 0 (HighPerformance) hoặc 1 (PowerSaving)
        private static int ClampDeviceProfile(int value) => value is 0 or 1 ? value : 0;

        // Trich code tu nhieu dinh dang: "QR-XXX", "/access?code=QR-XXX", "http://.../access?code=QR-XXX"
        private static string ExtractQrCode(string raw)
        {
            if (raw.Contains("code="))
            {
                var match = System.Text.RegularExpressions.Regex.Match(raw, @"code=([^&\s]+)");
                if (match.Success) return Uri.UnescapeDataString(match.Groups[1].Value);
            }
            return raw.Trim();
        }

        // POST /api/access/pay
        // Mobile goi voi { qrCode, deviceId } -> tra ve paymentUrl (URL callback lam payment)
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PayRequest req)
        {
            var code = ExtractQrCode(req.QrCode);
            var qr = await _context.QrCodes
                .FirstOrDefaultAsync(q => q.QrCodeValue == code && q.IsActive);

            if (qr is null)
                return BadRequest(new { message = "QR code không hợp lệ hoặc đã bị vô hiệu hóa" });

            if (qr.ExpiredAt.HasValue && qr.ExpiredAt < DateTime.UtcNow)
                return BadRequest(new { message = "QR code đã hết hạn" });

            // Tra ve URL callback de mobile mo trong browser
            // Android emulator: 10.0.2.2 tro ve localhost cua may host
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var profileValue = ClampDeviceProfile(req.DeviceProfile);
            var paymentUrl = $"{baseUrl}/api/access/callback" +
                             $"?qrCode={Uri.EscapeDataString(req.QrCode)}" +
                             $"&deviceId={Uri.EscapeDataString(req.DeviceId)}" +
                             $"&deviceProfile={profileValue}";

            return Ok(new { paymentUrl });
        }

        // GET /api/access/callback?qrCode={}&deviceId={}
        // VNPay callback (hoac dev bypass) -> tao session -> redirect deep link
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromQuery] string qrCode,
            [FromQuery] string deviceId,
            [FromQuery] int deviceProfile = 0)
        {
            var code = ExtractQrCode(qrCode);
            var qr = await _context.QrCodes
                .FirstOrDefaultAsync(q => q.QrCodeValue == code && q.IsActive);

            if (qr is null)
                return BadRequest("QR code không hợp lệ");

            var profileValue = ClampDeviceProfile(deviceProfile);
            var session = new AccessSession
            {
                QrCodeID = qr.QrCodeID,
                DeviceID = deviceId,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(24),
                IsRevoked = false,
                DeviceProfile = profileValue
            };
            _context.AccessSessions.Add(session);
            await _context.SaveChangesAsync();

            // Redirect toi deep link de mo app
            return Redirect(
                $"audiotravelling://session" +
                $"?sessionId={session.SessionID}" +
                $"&deviceId={Uri.EscapeDataString(deviceId)}");
        }

        // POST /api/access/verify
        // Kiem tra session con hop le khong
        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
        {
            var session = await _context.AccessSessions.FindAsync(req.SessionId);

            if (session is null || session.DeviceID != req.DeviceId || session.IsRevoked)
                return Ok(new { valid = false, expiredAt = (DateTime?)null });

            return Ok(new
            {
                valid = session.ExpiredAt > DateTime.UtcNow,
                expiredAt = session.ExpiredAt
            });
        }

        // GET /api/access/bootstrap
        // Headers: X-Session-Id (int), X-Device-Id (string)
        // Tra ve tat ca POI da approved + localizations cua chung
        [HttpGet("bootstrap")]
        public async Task<IActionResult> Bootstrap()
        {
            if (!int.TryParse(Request.Headers["X-Session-Id"], out var sessionId))
                return Unauthorized();

            var deviceId = Request.Headers["X-Device-Id"].ToString();

            var session = await _context.AccessSessions.FindAsync(sessionId);
            if (session is null
                || session.DeviceID != deviceId
                || session.IsRevoked
                || session.ExpiredAt <= DateTime.UtcNow)
                return Unauthorized();

            var pois = await _context.Pois
                .Include(p => p.Package)
                .Include(p => p.Localizations)
                .Include(p => p.Images)
                .Where(p => p.Status == "Approved" && p.IsActive)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            string BuildAbsoluteUrl(string? url)
            {
                if (string.IsNullOrWhiteSpace(url)) return string.Empty;
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return url;
                return baseUrl + (url.StartsWith("/") ? url : "/" + url);
            }

            return Ok(new
            {
                pois = pois.Select(p => new
                {
                    p.PoiID,
                    p.PoiName,
                    p.DescriptionVi,
                    p.Latitude,
                    p.Longitude,
                    Radius = p.Package.Radius,
                    Priority = p.Package.Priority,
                    p.IsActive,
                    CoverImageUrl = BuildAbsoluteUrl(
                        p.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl
                        ?? p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl),
                    p.UpdatedDate
                }),
                localizations = pois
                    .SelectMany(p => p.Localizations)
                    .Select(l => new
                    {
                        l.LocalizationID,
                        l.PoiID,
                        l.LanguageCode,
                        l.Name,
                        l.Description,
                        l.AudioUrl,
                        l.UpdatedDate
                    })
            });
        }

        // GET /api/access/pois/{id}/images
        // Mobile online: lay tat ca anh cua 1 POI (de hien thi carousel)
        [HttpGet("pois/{id}/images")]
        public async Task<IActionResult> GetPoiImages(int id)
        {
            if (!int.TryParse(Request.Headers["X-Session-Id"], out var sessionId))
                return Unauthorized();

            var deviceId = Request.Headers["X-Device-Id"].ToString();
            var session = await _context.AccessSessions.FindAsync(sessionId);
            if (session is null || session.DeviceID != deviceId || session.IsRevoked || session.ExpiredAt <= DateTime.UtcNow)
                return Unauthorized();

            var images = await _context.PoiImages
                .Where(i => i.PoiID == id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            string BuildAbsoluteUrl(string? url)
            {
                if (string.IsNullOrWhiteSpace(url)) return string.Empty;
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return url;
                return baseUrl + (url.StartsWith("/") ? url : "/" + url);
            }

            return Ok(images.Select(i => new PoiImageDto
            {
                ImageID = i.ImageID,
                ImageUrl = BuildAbsoluteUrl(i.ImageUrl),
                IsCover = i.IsCover,
                DisplayOrder = i.DisplayOrder
            }));
        }

        // POST /api/access/dev-bypass
        // Chi dung khi dev/test: tao session that voi device ID thuc te, khong can quet QR
        [HttpPost("dev-bypass")]
        public async Task<IActionResult> DevBypass([FromBody] DevBypassRequest req)
        {
            var qr = await _context.QrCodes
                .FirstOrDefaultAsync(q => q.IsActive);

            if (qr is null)
                return BadRequest(new { message = "Không có QR code active nào trong hệ thống" });

            var profileValue = ClampDeviceProfile(req.DeviceProfile);
            var session = new AccessSession
            {
                QrCodeID = qr.QrCodeID,
                DeviceID = req.DeviceId,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(24),
                IsRevoked = false,
                DeviceProfile = profileValue
            };
            _context.AccessSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { sessionId = session.SessionID, expiredAt = session.ExpiredAt });
        }
    }

    public record PayRequest(string QrCode, string DeviceId, int DeviceProfile = 0);
    public record VerifyRequest(int SessionId, string DeviceId);
    public record DevBypassRequest(string DeviceId, int DeviceProfile = 0);
}
