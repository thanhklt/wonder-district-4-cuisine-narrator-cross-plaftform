using AudioTravelling.API.DTOs;
using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/qr")]
[Authorize(Roles = "Admin")]
public class QrController(IAppDbContext db, IConfiguration config) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var codes = await db.AccessCodes
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new QrCodeResponse(c.Id, c.Code, c.QrImageUrl, c.IsActive, c.CreatedAt))
            .ToListAsync();
        return Ok(codes);
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var code = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var qrUrl = $"/qr/{code}.png";

        var storagePath = config["QR_STORAGE_PATH"] ?? "/storage/qr";
        Directory.CreateDirectory(storagePath);

        var mobileAppUrl = config["MOBILE_APP_URL"] ?? "http://localhost:3000";
        var qrContent = $"{mobileAppUrl}/pay?code={code}";

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(storagePath, $"{code}.png"), pngBytes);

        var accessCode = new AccessCode { Code = code, QrImageUrl = qrUrl };
        db.AccessCodes.Add(accessCode);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = accessCode.Id },
            new QrCreateResponse(accessCode.Id, accessCode.Code, accessCode.QrImageUrl, accessCode.IsActive));
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var code = await db.AccessCodes.FindAsync(id);
        if (code is null) return NotFound();
        code.IsActive = !code.IsActive;
        await db.SaveChangesAsync();
        return Ok(new QrToggleResponse(code.Id, code.IsActive));
    }

    [HttpPost("{id:guid}/regenerate")]
    public async Task<IActionResult> Regenerate(Guid id)
    {
        var code = await db.AccessCodes.FindAsync(id);
        if (code is null) return NotFound();

        var storagePath = config["QR_STORAGE_PATH"] ?? "/storage/qr";
        Directory.CreateDirectory(storagePath);

        var mobileAppUrl = config["MOBILE_APP_URL"] ?? "http://localhost:3000";
        var qrContent = $"{mobileAppUrl}/pay?code={code.Code}";

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(storagePath, $"{code.Code}.png"), pngBytes);

        return Ok(new QrCreateResponse(code.Id, code.Code, code.QrImageUrl, code.IsActive));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var code = await db.AccessCodes.FindAsync(id);
        if (code is null) return NotFound();

        var storagePath = config["QR_STORAGE_PATH"] ?? "/storage/qr";
        var filePath = Path.Combine(storagePath, $"{code.Code}.png");
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        db.AccessCodes.Remove(code);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
