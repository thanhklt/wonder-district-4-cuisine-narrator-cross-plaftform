using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("AppUsers")]
public class AppUserCache
{
    [PrimaryKey]
    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PreferredLanguage { get; set; } = "en";

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenExpiresAtUtc { get; set; } = string.Empty;

    public string LastLoginAtUtc { get; set; } = string.Empty;

    public int IsActive { get; set; }

    public string UpdatedAtUtc { get; set; } = string.Empty;
}