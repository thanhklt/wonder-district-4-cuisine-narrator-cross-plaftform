using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/admin/qr")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminQrController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminQrController(AppDbContext context)
        {
            _context = context;
        }

        private string GenerateRandomCode()
        {
            return "QR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var codes = await _context.QrCodes
                .Include(c => c.Sessions)
                .Where(c => c.IsActive)
                .Select(c => new QrDto
                {
                    QrId = c.QrCodeID,
                    Code = c.QrCodeValue,
                    TargetUrl = $"/access?code={c.QrCodeValue}",
                    QrPayload = $"/access?code={c.QrCodeValue}",
                    QrImageUrl = null,
                    Status = c.IsActive ? "Active" : "Inactive",
                    IsActive = c.IsActive,
                    ExpiredAt = c.ExpiredAt,
                    CreatedDate = c.CreatedDate,
                    ScanCount = c.Sessions.Count
                })
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return Ok(codes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.QrCodes
                .Include(ac => ac.Sessions)
                .FirstOrDefaultAsync(ac => ac.QrCodeID == id);

            if (c == null) return NotFound();

            return Ok(new QrDto
            {
                QrId = c.QrCodeID,
                Code = c.QrCodeValue,
                TargetUrl = $"/access?code={c.QrCodeValue}",
                QrPayload = $"/access?code={c.QrCodeValue}",
                QrImageUrl = null,
                Status = c.IsActive ? "Active" : "Inactive",
                IsActive = c.IsActive,
                ExpiredAt = c.ExpiredAt,
                CreatedDate = c.CreatedDate,
                ScanCount = c.Sessions.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQrRequest request)
        {
            var code = new QrCode
            {
                QrCodeValue = GenerateRandomCode(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ExpiredAt = request.ExpiredAt
            };

            _context.QrCodes.Add(code);
            await _context.SaveChangesAsync();

            return Ok(new QrDto
            {
                QrId = code.QrCodeID,
                Code = code.QrCodeValue,
                TargetUrl = $"/access?code={code.QrCodeValue}",
                QrPayload = $"/access?code={code.QrCodeValue}",
                Status = "Active",
                IsActive = true,
                ExpiredAt = code.ExpiredAt,
                CreatedDate = code.CreatedDate,
                ScanCount = 0
            });
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var c = await _context.QrCodes.FindAsync(id);
            if (c == null) return NotFound();

            c.IsActive = !c.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Toggled successfully", isActive = c.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.QrCodes.Include(ac => ac.Sessions).FirstOrDefaultAsync(ac => ac.QrCodeID == id);
            if (c == null) return NotFound();

            if (c.Sessions.Any())
            {
                // Soft delete
                c.IsActive = false;
            }
            else
            {
                // Hard delete if no sessions
                _context.QrCodes.Remove(c);
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted successfully" });
        }

        [HttpPost("{id}/regenerate")]
        public async Task<IActionResult> Regenerate(int id)
        {
            var c = await _context.QrCodes.Include(ac => ac.Sessions).FirstOrDefaultAsync(ac => ac.QrCodeID == id);
            if (c == null) return NotFound();

            c.QrCodeValue = GenerateRandomCode();
            await _context.SaveChangesAsync();

            return Ok(new QrDto
            {
                QrId = c.QrCodeID,
                Code = c.QrCodeValue,
                TargetUrl = $"/access?code={c.QrCodeValue}",
                QrPayload = $"/access?code={c.QrCodeValue}",
                Status = c.IsActive ? "Active" : "Inactive",
                IsActive = c.IsActive,
                ExpiredAt = c.ExpiredAt,
                CreatedDate = c.CreatedDate,
                ScanCount = c.Sessions.Count
            });
        }
    }

    public class QrDto
    {
        public int QrId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string QrPayload { get; set; } = string.Empty;
        public string? QrImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ScanCount { get; set; }
    }

    public class CreateQrRequest
    {
        public DateTime? ExpiredAt { get; set; }
    }
}
