namespace AudioTravelling.Mobile.Services.Api.Exceptions;

public class ApiException : Exception
{
    public string UserMessage { get; }
    public int? StatusCode { get; }

    public ApiException(string userMessage, string? technicalMessage = null, int? statusCode = null)
        : base(technicalMessage ?? userMessage)
    {
        UserMessage = userMessage;
        StatusCode = statusCode;
    }
}