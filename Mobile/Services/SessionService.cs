using Mobile.Models;
using System.Security.Cryptography;
using System.Text;

namespace Mobile.Services;

public class SessionService
{
    private readonly DatabaseService _db;

    public SessionService(DatabaseService db) => _db = db;

    public string GetDeviceId()
    {
        var raw = $"{DeviceInfo.Current.Name}{DeviceInfo.Current.Model}{DeviceInfo.Current.Platform}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..32].ToLower();
    }

    public async Task<CachedAccessSession?> GetValidSessionAsync()
    {
        var deviceId = GetDeviceId();
        var session = await _db.GetValidSessionAsync(deviceId);
        return session?.IsValid == true ? session : null;
    }

    public async Task SaveSessionAsync(int sessionId, DateTime expiredAt)
    {
        var deviceId = GetDeviceId();
        var session = new CachedAccessSession
        {
            SessionID = sessionId,
            DeviceID = deviceId,
            IssuedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt,
            IsRevoked = false,
            CachedAt = DateTime.UtcNow
        };
        await _db.SaveSessionAsync(session);
    }

    public async Task InvalidateAsync() =>
        await _db.InvalidateSessionAsync(GetDeviceId());
}
