namespace AudioTravelling.Mobile.Services.Api;

public static class ApiUrlHelper
{
    public const string BaseUrl = "http://10.0.2.2:5184";

    public static string? ToAbsoluteUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        url = url.Trim();

        if (url.Contains("api.audiotravelling.com", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(url);
            return $"{BaseUrl.TrimEnd('/')}{uri.AbsolutePath}";
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return $"{BaseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
    }
}