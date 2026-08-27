using System.Net;

namespace PostForge.Providers.TikTok;

public sealed class TikTokApiException : Exception
{
    public TikTokApiException(string message, int? errorCode = null, string? logId = null, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        LogId = logId;
        StatusCode = statusCode;
    }

    public int? ErrorCode { get; }
    public string? LogId { get; }
    public HttpStatusCode? StatusCode { get; }
}
