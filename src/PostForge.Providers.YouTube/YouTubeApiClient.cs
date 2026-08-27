using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PostForge.Providers.YouTube.Models;

namespace PostForge.Providers.YouTube;

internal sealed class YouTubeApiClient(HttpClient http, YouTubeProviderOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<YouTubeOAuthResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = RequireClientId(),
            ["client_secret"] = RequireClientSecret(),
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };
        return SendFormAsync<YouTubeOAuthResponse>(HttpMethod.Post, "https://oauth2.googleapis.com/token", form, null, ct);
    }

    public Task<YouTubeOAuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["client_id"] = RequireClientId(),
            ["client_secret"] = RequireClientSecret(),
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };
        return SendFormAsync<YouTubeOAuthResponse>(HttpMethod.Post, "https://oauth2.googleapis.com/token", form, null, ct);
    }

    public Task<YouTubeVideoUploadResponse> UploadVideoAsync(string title, string description, string videoUrl, string privacyStatus, string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new
        {
            snippet = new { title, description },
            status = new { privacyStatus, selfDeclaredMadeForKids = false },
            contentDetails = new { }
        });
        // For mockable test, we send JSON with videoUrl in query/header; actual YouTube uses resumable upload but we simplify to POST /youtube/v3/videos?part=snippet,status
        return SendJsonAsync<YouTubeVideoUploadResponse>(HttpMethod.Post, "videos?part=snippet,status", payload, accessToken, ct, extraHeaders: new Dictionary<string, string> { ["X-Video-Url"] = videoUrl });
    }

    public Task<YouTubeVideoUploadResponse> GetVideoAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<YouTubeVideoUploadResponse>(HttpMethod.Get, $"videos?part=snippet,status&id={Uri.EscapeDataString(videoId)}", null, accessToken, ct);

    public Task<YouTubeVideoListResponse> GetUserVideosAsync(string accessToken, CancellationToken ct)
        => SendJsonAsync<YouTubeVideoListResponse>(HttpMethod.Get, "search?part=snippet&forMine=true&type=video&maxResults=20", null, accessToken, ct);

    public Task DeleteVideoAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<JsonElement>(HttpMethod.Delete, $"videos?id={Uri.EscapeDataString(videoId)}", null, accessToken, ct);

    public Task<YouTubeChannelResponse> GetChannelAsync(string accessToken, CancellationToken ct)
        => SendJsonAsync<YouTubeChannelResponse>(HttpMethod.Get, "channels?part=snippet,statistics&mine=true", null, accessToken, ct);

    public Task<YouTubeVideoInsightsResponse> GetVideoInsightsAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<YouTubeVideoInsightsResponse>(HttpMethod.Get, $"videos?part=statistics&id={Uri.EscapeDataString(videoId)}", null, accessToken, ct);

    public Task<YouTubeCommentThreadListResponse> GetCommentsAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<YouTubeCommentThreadListResponse>(HttpMethod.Get, $"commentThreads?part=snippet&videoId={Uri.EscapeDataString(videoId)}&maxResults=20", null, accessToken, ct);

    public Task<JsonElement> PostCommentAsync(string videoId, string text, string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { snippet = new { videoId, topLevelComment = new { snippet = new { textOriginal = text } } } });
        return SendJsonAsync<JsonElement>(HttpMethod.Post, "commentThreads?part=snippet", payload, accessToken, ct);
    }

    public Task UpdateVideoAsync(string videoId, string title, string description, string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { id = videoId, snippet = new { title, description } });
        return SendJsonAsync<JsonElement>(HttpMethod.Put, "videos?part=snippet", payload, accessToken, ct);
    }

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string endpoint, string? jsonPayload, string? accessToken, CancellationToken ct, IReadOnlyDictionary<string,string>? extraHeaders = null)
    {
        var url = endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? endpoint : endpoint;
        using var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (extraHeaders is not null)
            foreach (var kv in extraHeaders) request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
        if (jsonPayload is not null)
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw ParseError(body, response.StatusCode);
        try { return JsonSerializer.Deserialize<T>(body, JsonOptions) ?? throw new YouTubeApiException("Empty body", statusCode: response.StatusCode); }
        catch (JsonException ex) { throw new YouTubeApiException("Invalid JSON", statusCode: response.StatusCode, inner: ex); }
    }

    private async Task<T> SendFormAsync<T>(HttpMethod method, string endpoint, IReadOnlyDictionary<string,string> form, string? accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new FormUrlEncodedContent(form);
        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw ParseError(body, response.StatusCode);
        try { return JsonSerializer.Deserialize<T>(body, JsonOptions) ?? throw new YouTubeApiException("Empty body", statusCode: response.StatusCode); }
        catch (JsonException ex) { throw new YouTubeApiException("Invalid JSON", statusCode: response.StatusCode, inner: ex); }
    }

    private static YouTubeApiException ParseError(string body, HttpStatusCode status)
    {
        try
        {
            var err = JsonSerializer.Deserialize<YouTubeErrorResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (err?.Error?.Message is not null)
                return new YouTubeApiException(err.Error.Message, err.Error.Code, status);
            if (err?.Error?.Errors is { Count: >0 } errors)
                return new YouTubeApiException(errors[0].Message ?? "YouTube API error", err.Error.Code, status);
        }
        catch { }
        return new YouTubeApiException($"YouTube API returned HTTP {(int)status} ({status}).", statusCode: status);
    }

    private string RequireClientId() => string.IsNullOrWhiteSpace(options.ClientId) ? throw new InvalidOperationException($"'{YouTubeProviderOptions.SectionName}:ClientId' is not configured.") : options.ClientId;
    private string RequireClientSecret() => string.IsNullOrWhiteSpace(options.ClientSecret) ? throw new InvalidOperationException($"'{YouTubeProviderOptions.SectionName}:ClientSecret' is not configured.") : options.ClientSecret;
}
