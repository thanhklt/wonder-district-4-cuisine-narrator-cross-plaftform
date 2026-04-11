using System.Diagnostics;
using AudioTravelling.Mobile.Services.Auth;

namespace AudioTravelling.Mobile.Services.Api.Handlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    public AuthHeaderHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Debug.WriteLine($"[AuthHeaderHandler] ✅ Token added: {token.Substring(0, Math.Min(20, token.Length))}...");
        }
        else
        {
            Debug.WriteLine($"[AuthHeaderHandler] ⚠️ No token available");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
