using System.Diagnostics;

namespace AudioTravelling.Mobile.Services.Api.Handlers;

public class LoggingHandler : DelegatingHandler
{
    private readonly bool _enableLogging;

    public LoggingHandler(bool enableLogging = true)
    {
        _enableLogging = enableLogging;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_enableLogging)
        {
            Debug.WriteLine($"[API Request] {request.Method} {request.RequestUri}");
            
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"[API Request Body] {content}");
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (_enableLogging)
        {
            Debug.WriteLine($"[API Response] {(int)response.StatusCode} {response.StatusCode}");
            
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(content))
                {
                    Debug.WriteLine($"[API Response Body] {content}");
                }
            }
        }

        return response;
    }
}
