using AudioTravelling.Mobile.Services.Api.Exceptions;
using System.Net;
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

            EnsureSuccess(response, raw, endpoint, "GET");

            if (string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new ApiException(
                    "Dữ liệu trả về không hợp lệ. Vui lòng thử lại sau.",
                    $"Không parse được response từ GET {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(
                "Kết nối đang chậm. Vui lòng thử lại.",
                $"Request GET {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                "Không thể kết nối đến máy chủ. Vui lòng kiểm tra mạng và thử lại.",
                $"Không gọi được API GET {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ApiException(
                "Có lỗi xảy ra. Vui lòng thử lại sau.",
                $"Lỗi không xác định ở GET {endpoint}: {ex}");
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

            EnsureSuccess(response, raw, endpoint, "POST");

            if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new ApiException(
                    "Dữ liệu trả về không hợp lệ. Vui lòng thử lại sau.",
                    $"Không parse được response từ POST {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(
                "Kết nối đang chậm. Vui lòng thử lại.",
                $"Request POST {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                "Không thể kết nối đến máy chủ. Vui lòng kiểm tra mạng và thử lại.",
                $"Không gọi được API POST {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ApiException(
                "Có lỗi xảy ra. Vui lòng thử lại sau.",
                $"Lỗi không xác định ở POST {endpoint}: {ex}");
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

            EnsureSuccess(response, raw, endpoint, "PUT");

            if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(raw))
                return default;

            var result = JsonSerializer.Deserialize<T>(raw, JsonOptions);

            if (result == null)
            {
                throw new ApiException(
                    "Dữ liệu trả về không hợp lệ. Vui lòng thử lại sau.",
                    $"Không parse được response từ PUT {endpoint}. Raw: {raw}");
            }

            return result;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(
                "Kết nối đang chậm. Vui lòng thử lại.",
                $"Request PUT {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                "Không thể kết nối đến máy chủ. Vui lòng kiểm tra mạng và thử lại.",
                $"Không gọi được API PUT {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ApiException(
                "Có lỗi xảy ra. Vui lòng thử lại sau.",
                $"Lỗi không xác định ở PUT {endpoint}: {ex}");
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

            EnsureSuccess(response, raw, endpoint, "DELETE");
            return true;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(
                "Kết nối đang chậm. Vui lòng thử lại.",
                $"Request DELETE {endpoint} bị timeout.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                "Không thể kết nối đến máy chủ. Vui lòng kiểm tra mạng và thử lại.",
                $"Không gọi được API DELETE {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ApiException(
                "Có lỗi xảy ra. Vui lòng thử lại sau.",
                $"Lỗi không xác định ở DELETE {endpoint}: {ex}");
        }
    }

    private static void EnsureSuccess(HttpResponseMessage response, string raw, string endpoint, string method)
    {
        if (response.IsSuccessStatusCode)
            return;

        var statusCode = (int)response.StatusCode;
        var userMessage = MapUserMessage(response.StatusCode, raw);

        throw new ApiException(
            userMessage,
            $"{method} {endpoint} thất bại. Status: {statusCode} ({response.StatusCode}). Body: {raw}",
            statusCode);
    }

    private static string MapUserMessage(HttpStatusCode statusCode, string raw)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => ExtractMessageFromBody(raw) ?? "Thông tin gửi lên chưa hợp lệ.",
            HttpStatusCode.Unauthorized => "Email hoặc mật khẩu chưa đúng.",
            HttpStatusCode.Forbidden => "Bạn không có quyền thực hiện thao tác này.",
            HttpStatusCode.NotFound => "Không tìm thấy dữ liệu yêu cầu.",
            HttpStatusCode.Conflict => ExtractMessageFromBody(raw) ?? "Dữ liệu đã tồn tại.",
            HttpStatusCode.InternalServerError => "Máy chủ đang gặp sự cố. Vui lòng thử lại sau.",
            _ => "Có lỗi xảy ra. Vui lòng thử lại sau."
        };
    }

    private static string? ExtractMessageFromBody(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var message))
                return message.GetString();

            if (root.TryGetProperty("error", out var error))
                return error.GetString();

            if (root.TryGetProperty("title", out var title))
                return title.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }
}