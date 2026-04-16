namespace Api.Modules.Audio.Services;

public static class VoiceMapper
{
    public static string GetVoice(string lang) => lang switch
    {
        "vi" => "vi-VN-HoaiMyNeural",
        "en" => "en-US-AriaNeural",
        "zh" => "zh-CN-XiaoxiaoNeural",
        "ja" => "ja-JP-NanamiNeural",
        "ru" => "ru-RU-SvetlanaNeural",
        "fr" => "fr-FR-DeniseNeural",
        "ko" => "ko-KR-SunHiNeural",
        "de" => "de-DE-KatjaNeural",
        _ => "en-US-AriaNeural"
    };
}