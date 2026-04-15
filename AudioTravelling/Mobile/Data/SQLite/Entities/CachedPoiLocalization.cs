using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("CachedPoiLocalizations")]
public class CachedPoiLocalization
{
    [PrimaryKey]
    public int LocalizationId { get; set; }

    public int PoiId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TranslationSource { get; set; } = string.Empty;

    public string ServerUpdatedAtUtc { get; set; } = string.Empty;

    public string CachedAtUtc { get; set; } = string.Empty;
}