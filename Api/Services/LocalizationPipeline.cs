using Api.DTOs;
using Api.Models.Entities;
using Api.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Api.Services
{
    public class LocalizationPipeline
    {
        private readonly AppDbContext _context;
        private readonly LocalizeService _localizer;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _ttsServiceUrl;
        private readonly string _webRootPath;

        private static readonly Dictionary<string, string> VoiceMap = new()
        {
            ["vi"] = "vi-VN-NamMinhNeural",
            ["en"] = "en-US-ChristopherNeural",
            ["zh"] = "zh-CN-YunjianNeural",
            ["ja"] = "ja-JP-KeitaNeural",
            ["ru"] = "ru-RU-DmitryNeural"
        };

        public LocalizationPipeline(
            AppDbContext context,
            LocalizeService localizer,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _context = context;
            _localizer = localizer;
            _httpFactory = httpFactory;
            _ttsServiceUrl = config["TtsServiceUrl"] ?? "http://localhost:8000";
            _webRootPath = env.WebRootPath
                ?? Path.Combine(env.ContentRootPath, "wwwroot");
        }

        public async Task LocalizePoiAsync(int poiId)
        {
            var poi = await _context.Pois.FindAsync(poiId);
            if (poi is null) return;

            var viText = poi.DescriptionVi;
            var poiName = poi.PoiName;

            foreach (var (lang, voice) in VoiceMap)
            {
                try
                {
                    var text = viText;
                    if (lang != "vi")
                    {
                        var translated = await _localizer.TranslateAsync(new TranslateRequest
                        {
                            Text = viText,
                            SourceLanguage = "vi",
                            TargetLanguage = lang
                        });
                        if (translated is null) continue;
                        text = translated.TranslatedText;
                    }

                    var audioBytes = await CallTtsAsync(text, voice, lang);
                    if (audioBytes is null) continue;

                    var dir = Path.Combine(_webRootPath, "audio", poiId.ToString());
                    Directory.CreateDirectory(dir);
                    await File.WriteAllBytesAsync(Path.Combine(dir, $"{lang}.mp3"), audioBytes);

                    var audioUrl = $"/audio/{poiId}/{lang}.mp3";
                    var existing = await _context.PoiLocalizations
                        .FirstOrDefaultAsync(l => l.PoiID == poiId && l.LanguageCode == lang);

                    if (existing is not null)
                    {
                        existing.Name = poiName;
                        existing.Description = text;
                        existing.AudioUrl = audioUrl;
                        existing.UpdatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.PoiLocalizations.Add(new PoiLocalization
                        {
                            PoiID = poiId,
                            LanguageCode = lang,
                            Name = poiName,
                            Description = text,
                            AudioUrl = audioUrl,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                    }
                }
                catch { /* lang failure does not abort others */ }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<byte[]?> CallTtsAsync(string text, string voice, string lang)
        {
            try
            {
                var http = _httpFactory.CreateClient();
                var res = await http.PostAsJsonAsync(
                    $"{_ttsServiceUrl}/tts",
                    new { text, voice, lang });
                if (!res.IsSuccessStatusCode) return null;
                return await res.Content.ReadAsByteArrayAsync();
            }
            catch { return null; }
        }
    }
}
