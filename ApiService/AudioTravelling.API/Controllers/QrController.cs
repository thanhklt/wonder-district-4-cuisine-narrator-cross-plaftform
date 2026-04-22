using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/qr")]
[Authorize(Roles = "Admin")]
public class QrController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var codes = await db.AccessCodes
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new { c.Id, c.Code, c.QrImageUrl, c.IsActive, c.CreatedAt })
            .ToListAsync();
        return Ok(codes);
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var code = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var qrUrl = $"/qr/{code}.png";

        var accessCode = new AccessCode { Code = code, QrImageUrl = qrUrl };
        db.AccessCodes.Add(accessCode);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = accessCode.Id },
            new { accessCode.Id, accessCode.Code, accessCode.QrImageUrl, accessCode.IsActive });
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var code = await db.AccessCodes.FindAsync(id);
        if (code is null) return NotFound();
        code.IsActive = !code.IsActive;
        await db.SaveChangesAsync();
        return Ok(new { code.Id, code.IsActive });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var code = await db.AccessCodes.FindAsync(id);
        if (code is null) return NotFound();
        db.AccessCodes.Remove(code);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
