using Api.DTOs;
using Api.Models.Entities;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Api.Controllers
{
    [Route("api/tts")]
    [ApiController]
    public class TtsProxyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LocalizeService _localizer;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IWebHostEnvironment _env;
        private readonly string _ttsServiceUrl;

        private static readonly Dictionary<string, string> VoiceMap = new()
        {
            ["vi"] = "vi-VN-NamMinhNeural",
            ["en"] = "en-US-ChristopherNeural",
            ["zh"] = "zh-CN-YunjianNeural",
            ["ja"] = "ja-JP-KeitaNeural",
            ["ru"] = "ru-RU-DmitryNeural",
            ["ko"] = "ko-KR-GookMinNeural",
            ["fr"] = "fr-FR-HenriNeural",
            ["de"] = "de-DE-ConradNeural",
            ["es"] = "es-ES-AlvaroNeural",
            ["it"] = "it-IT-DiegoNeural",
        };

        public TtsProxyController(
            AppDbContext context,
            LocalizeService localizer,
            IHttpClientFactory httpFactory,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _context = context;
            _localizer = localizer;
            _httpFactory = httpFactory;
            _env = env;
            _ttsServiceUrl = config["TtsServiceUrl"] ?? "http://localhost:8000";
        }

        // POST /api/tts/proxy
        // Tao audio on-demand, luu file + cap nhat PoiLocalization de tai su dung
        [HttpPost("proxy")]
        public async Task<IActionResult> Proxy([FromBody] TtsProxyRequest req)
        {
            if (!VoiceMap.TryGetValue(req.LangCode, out var voice))
                return BadRequest(new { message = $"Ngôn ngữ '{req.LangCode}' không được hỗ trợ" });

            // Kiem tra xem da co audio san chua (tránh tạo lại)
            var existing = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.PoiID == req.PoiId && l.LanguageCode == req.LangCode);

            if (existing is not null && !string.IsNullOrEmpty(existing.AudioUrl))
            {
                var cachedPath = Path.Combine(
                    _env.WebRootPath ?? "wwwroot",
                    existing.AudioUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(cachedPath))
                    return File(System.IO.File.ReadAllBytes(cachedPath), "audio/mpeg");
            }

            // Lay poi de co ten
            var poi = await _context.Pois.FindAsync(req.PoiId);
            if (poi is null) return NotFound(new { message = "POI không tồn tại" });

            // Lay text goc tieng Viet
            var viLoc = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.PoiID == req.PoiId && l.LanguageCode == "vi");
            var sourceText = (viLoc is not null && !string.IsNullOrEmpty(viLoc.Description))
                ? viLoc.Description
                : poi.DescriptionVi;

            // Dich sang ngon ngu yeu cau
            var translatedText = sourceText;
            var translatedName = poi.PoiName;

            if (req.LangCode != "vi")
            {
                var translated = await _localizer.TranslateAsync(new TranslateRequest
                {
                    Text = sourceText,
                    SourceLanguage = "vi",
                    TargetLanguage = req.LangCode
                });
                if (translated is null)
                    return StatusCode(500, new { message = "Không thể dịch văn bản" });
                translatedText = translated.TranslatedText;

                // Dich ten POI
                var nameTranslated = await _localizer.TranslateAsync(new TranslateRequest
                {
                    Text = poi.PoiName,
                    SourceLanguage = "vi",
                    TargetLanguage = req.LangCode
                });
                translatedName = nameTranslated?.TranslatedText ?? poi.PoiName;
            }

            // Goi TTSService
            byte[] audioBytes;
            try
            {
                var http = _httpFactory.CreateClient();
                var res = await http.PostAsJsonAsync(
                    $"{_ttsServiceUrl}/tts",
                    new { text = translatedText, voice, lang = req.LangCode });

                if (!res.IsSuccessStatusCode)
                    return StatusCode(502, new { message = "TTS service trả về lỗi" });

                audioBytes = await res.Content.ReadAsByteArrayAsync();
            }
            catch
            {
                return StatusCode(500, new { message = "TTS service không khả dụng" });
            }

            // Luu file audio vao storage
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var dir = Path.Combine(webRoot, "audio", req.PoiId.ToString());
                Directory.CreateDirectory(dir);
                var filePath = Path.Combine(dir, $"{req.LangCode}.mp3");
                await System.IO.File.WriteAllBytesAsync(filePath, audioBytes);

                var audioUrl = $"/audio/{req.PoiId}/{req.LangCode}.mp3";

                // Upsert PoiLocalization
                if (existing is not null)
                {
                    existing.Name        = translatedName;
                    existing.Description = translatedText;
                    existing.AudioUrl    = audioUrl;
                    existing.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.PoiLocalizations.Add(new PoiLocalization
                    {
                        PoiID        = req.PoiId,
                        LanguageCode = req.LangCode,
                        Name         = translatedName,
                        Description  = translatedText,
                        AudioUrl     = audioUrl,
                        CreatedDate  = DateTime.UtcNow,
                        UpdatedDate  = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Loi luu file/DB khong anh huong den viec tra audio cho client
            }

            return File(audioBytes, "audio/mpeg");
        }
    }

    public record TtsProxyRequest(int PoiId, string LangCode);
}
