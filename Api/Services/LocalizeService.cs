using GTranslate.Translators;
using Api.DTOs;

namespace Api.Services
{
    public class LocalizeService
    {
        private readonly GoogleTranslator _translator = new();

        public async Task<TranslateResponse?> TranslateAsync(TranslateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text)) return null;

            var sourceLang = request.SourceLanguage.Split('-')[0];
            var targetLang = request.TargetLanguage.Split('-')[0];

            if (sourceLang == targetLang)
                return new TranslateResponse
                {
                    TranslatedText = request.Text,
                    SourceLanguage = sourceLang,
                    TargetLanguage = targetLang,
                    Provider = "google"
                };

            var result = await _translator.TranslateAsync(
                request.Text,
                targetLang,
                sourceLang
            );

            return new TranslateResponse
            {
                TranslatedText = result.Translation,
                SourceLanguage = sourceLang,
                TargetLanguage = targetLang,
                Provider = "google"
            };
        }
    }
}
