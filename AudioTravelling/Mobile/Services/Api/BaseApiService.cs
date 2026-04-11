using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioTravelling.Mobile.Services.Api;

public abstract class BaseApiService
{
    protected readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected BaseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    protected async Task<T?> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"GET {endpoint} thất bại. Status: {(int)response.StatusCode} ({response.StatusCode}). Body: {raw}");
            }

            if (string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new Exception(
                    $"Không parse được response từ GET {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Request GET {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Không gọi được API GET {endpoint}: {ex.Message}");
        }
    }

    protected async Task<T?> PostAsync<T>(
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response;

            if (data != null)
            {
                response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonOptions, cancellationToken);
            }
            else
            {
                response = await _httpClient.PostAsync(endpoint, null, cancellationToken);
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"POST {endpoint} thất bại. Status: {(int)response.StatusCode} ({response.StatusCode}). Body: {raw}");
            }

            if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new Exception(
                    $"Không parse được response từ POST {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Request POST {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Không gọi được API POST {endpoint}: {ex.Message}");
        }
    }

    protected async Task<T?> PutAsync<T>(
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response;

            if (data != null)
            {
                response = await _httpClient.PutAsJsonAsync(endpoint, data, JsonOptions, cancellationToken);
            }
            else
            {
                response = await _httpClient.PutAsync(endpoint, null, cancellationToken);
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"PUT {endpoint} thất bại. Status: {(int)response.StatusCode} ({response.StatusCode}). Body: {raw}");
            }

            if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new Exception(
                    $"Không parse được response từ PUT {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Request PUT {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Không gọi được API PUT {endpoint}: {ex.Message}");
        }
    }

    protected async Task<bool> DeleteAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"DELETE {endpoint} thất bại. Status: {(int)response.StatusCode} ({response.StatusCode}). Body: {raw}");
            }

            return true;
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Request DELETE {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Không gọi được API DELETE {endpoint}: {ex.Message}");
        }
    }
}