using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Interfaces;
using AudioTravelling.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/access")]
public class AccessController(IAppDbContext db, VnPayService vnPay, IConfiguration config) : ControllerBase
{
    [HttpPost("pay")]
    public async Task<IActionResult> InitiatePayment([FromBody] PayRequest req)
    {
        var accessCode = await db.AccessCodes
            .FirstOrDefaultAsync(c => c.Code == req.Code && c.IsActive);

        if (accessCode is null)
            return BadRequest(new { message = "QR code không hợp lệ hoặc đã bị vô hiệu hóa" });

        var txnRef = Guid.NewGuid().ToString("N")[..12];
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var paymentUrl = vnPay.CreatePaymentUrl(req.Code, 10000, ip, txnRef);

        return Ok(new { paymentUrl, txnRef });
    }

    [HttpGet("callback")]
    public async Task<IActionResult> VnPayCallback([FromQuery] Dictionary<string, string> query)
    {
        if (!vnPay.ValidateCallback(query))
            return BadRequest(new { message = "Invalid signature" });

        if (query.GetValueOrDefault("vnp_ResponseCode") != "00")
            return BadRequest(new { message = "Payment failed" });

        var orderInfo = query.GetValueOrDefault("vnp_OrderInfo") ?? "";
        var code = orderInfo.Split(' ').LastOrDefault() ?? "";
        var accessCode = await db.AccessCodes.FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
        if (accessCode is null)
            return BadRequest(new { message = "Access code not found" });

        var sessionToken = Guid.NewGuid().ToString();
        var expiry = int.Parse(config["SESSION_EXPIRY_HOURS"] ?? "24");
        var session = new AccessSession
        {
            AccessCodeId = accessCode.Id,
            SessionToken = sessionToken,
            ExpiresAt = DateTime.UtcNow.AddHours(expiry),
            DeviceInfo = Request.Headers.UserAgent.ToString(),
        };
        db.AccessSessions.Add(session);
        await db.SaveChangesAsync();

        var returnUrl = $"{config["VNPAY_RETURN_URL"]?.Replace("/payment/callback", "")}?token={sessionToken}";
        return Redirect(returnUrl);
    }

    [HttpGet("bootstrap")]
    public async Task<IActionResult> Bootstrap([FromHeader(Name = "X-Session-Token")] string? token)
    {
        var session = await ValidateSession(token);
        if (session is null) return Unauthorized(new { message = "Session không hợp lệ hoặc đã hết hạn" });

        var pois = await db.Pois
            .Where(p => p.Status == Core.Enums.PoiStatus.Approved)
            .Include(p => p.Images)
            .Include(p => p.Localizations)
            .Select(p => new
            {
                p.Id, p.Name, p.Lat, p.Lng, p.RadiusMeters, p.Priority,
                Images = p.Images.OrderBy(i => i.Order).Select(i => new { i.ImageUrl }),
                Localizations = p.Localizations.Select(l => new { l.Language, l.TextContent, l.AudioUrl }),
            })
            .ToListAsync();

        return Ok(new { pois, sessionExpiresAt = session.ExpiresAt });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromHeader(Name = "X-Session-Token")] string? token)
    {
        var session = await ValidateSession(token);
        if (session is null) return Unauthorized();
        return Ok(new { valid = true, expiresAt = session.ExpiresAt });
    }

    private async Task<AccessSession?> ValidateSession(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        return await db.AccessSessions
            .FirstOrDefaultAsync(s => s.SessionToken == token && s.ExpiresAt > DateTime.UtcNow);
    }

    public record PayRequest(string Code);
}
