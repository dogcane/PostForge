using System.Net;
using System.Text;

namespace PostForge.Providers.Facebook.Tests;

internal sealed record RecordedRequest(
    HttpMethod Method,
    Uri? RequestUri,
    IReadOnlyDictionary<string, string>? Form);

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage>? _responder;
    private readonly Queue<HttpResponseMessage> _queue = new();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage>? responder = null)
        => _responder = responder;

    public List<RecordedRequest> Requests { get; } = new();

    public void Enqueue(HttpResponseMessage response) => _queue.Enqueue(response);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, string>? form = null;
        if (request.Content is not null)
            form = ParseParameters(await request.Content.ReadAsStringAsync(cancellationToken));

        Requests.Add(new RecordedRequest(request.Method, request.RequestUri, form));

        var response = _queue.Count > 0
            ? _queue.Dequeue()
            : _responder?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK);

        return response;
    }

    private static Dictionary<string, string> ParseParameters(string body)
    {
        var parameters = new Dictionary<string, string>();
        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            parameters[WebUtility.UrlDecode(parts[0])] = parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : string.Empty;
        }

        return parameters;
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}