using System.Collections.Concurrent;
using System.Net.Http.Json;
using AudioTravelling.API.DTOs;
using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/tts")]
public class TtsController(
    IAppDbContext db,
    IHttpClientFactory httpClientFactory,
    IConfiguration config) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    [HttpPost("proxy")]
    public async Task<IActionResult> Proxy(
        [FromHeader(Name = "X-Session-Token")] string? token,
        [FromBody] TtsProxyRequest req)
    {
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();
        var valid = await db.AccessSessions
            .AnyAsync(s => s.SessionToken == token && s.ExpiresAt > DateTime.UtcNow);
        if (!valid) return Unauthorized();

        if (!Guid.TryParse(req.PoiId, out var poiId))
            return BadRequest(new { message = "PoiId không hợp lệ" });

        var audioStorage = config["AUDIO_STORAGE_PATH"] ?? "/storage/audio";
        var audioFilePath = Path.Combine(audioStorage, req.PoiId, $"{req.Language}.mp3");

        // Lock per poi+language — tránh N request cùng generate 1 file
        var semaphore = _locks.GetOrAdd($"{req.PoiId}-{req.Language}", _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            // Đã có trong DB và file tồn tại → trả về ngay, không generate lại
            var existing = await db.PoiLocalizations
                .FirstOrDefaultAsync(l => l.PoiId == poiId && l.Language == req.Language);

            if (existing?.AudioUrl != null && System.IO.File.Exists(audioFilePath))
            {
                var existingBytes = await System.IO.File.ReadAllBytesAsync(audioFilePath);
                return File(existingBytes, "audio/mpeg");
            }

            // Chưa có → gọi TTS service generate và lưu file vào disk
            var ttsClient = httpClientFactory.CreateClient("TTS");
            var ttsResp = await ttsClient.PostAsJsonAsync("/tts/generate",
                new { poi_id = req.PoiId, language = req.Language, text = req.Text });

            if (!ttsResp.IsSuccessStatusCode)
                return StatusCode((int)ttsResp.StatusCode, "TTS service error");

            var ttsResult = await ttsResp.Content.ReadFromJsonAsync<TtsGenerateResult>();
            if (ttsResult?.audio_url is null)
                return StatusCode(500, "TTS không trả về audio_url");

            // Lưu/cập nhật PoiLocalization trong DB
            if (existing is null)
            {
                db.PoiLocalizations.Add(new PoiLocalization
                {
                    PoiId = poiId,
                    Language = req.Language,
                    TextContent = req.Text,
                    AudioUrl = ttsResult.audio_url,
                });
            }
            else
            {
                existing.AudioUrl = ttsResult.audio_url;
            }
            await db.SaveChangesAsync();

            var audioBytes = await System.IO.File.ReadAllBytesAsync(audioFilePath);
            return File(audioBytes, "audio/mpeg");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private record TtsGenerateResult(string audio_url);
}