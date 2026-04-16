namespace Api.Infrastructure.Helpers;

public static class LanguageHelper
{
    public static string Normalize(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
            return "vi";

        lang = lang.Trim().ToLowerInvariant();

        if (lang.StartsWith("vi")) return "vi";
        if (lang.StartsWith("en")) return "en";
        if (lang.StartsWith("zh")) return "zh";
        if (lang.StartsWith("ja")) return "ja";
        if (lang.StartsWith("ru")) return "ru";
        if (lang.StartsWith("fr")) return "fr";
        if (lang.StartsWith("ko")) return "ko";
        if (lang.StartsWith("de")) return "de";

        return "en";
    }

    public static bool IsDefaultLanguage(string lang)
    {
        return lang is "vi" or "en" or "zh" or "ja" or "ru";
    }
}