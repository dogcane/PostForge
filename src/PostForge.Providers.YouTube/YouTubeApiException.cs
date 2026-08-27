using System.Net;

namespace PostForge.Providers.YouTube;

public sealed class YouTubeApiException : Exception
{
    public YouTubeApiException(string message, int? errorCode = null, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
    public int? ErrorCode { get; }
    public HttpStatusCode? StatusCode { get; }
}
