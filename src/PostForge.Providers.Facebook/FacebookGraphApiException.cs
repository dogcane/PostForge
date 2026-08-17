using System.Net;

namespace PostForge.Providers.Facebook;

public sealed class FacebookGraphApiException : Exception
{
    public FacebookGraphApiException(
        string message,
        string? errorType = null,
        int? errorCode = null,
        int? errorSubcode = null,
        string? fbTraceId = null,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorType = errorType;
        ErrorCode = errorCode;
        ErrorSubcode = errorSubcode;
        FbTraceId = fbTraceId;
        StatusCode = statusCode;
    }

    public string? ErrorType { get; }

    public int? ErrorCode { get; }

    public int? ErrorSubcode { get; }

    public string? FbTraceId { get; }

    public HttpStatusCode? StatusCode { get; }
}
