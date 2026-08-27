using System.Net;
using System.Text;
using System.Text.Json;
using PostForge.Providers.TikTok.Models;

namespace PostForge.Providers.TikTok;

internal sealed class TikTokApiClient(HttpClient http, TikTokProviderOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<TikTokOAuthResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken ct)
    {
        var body = new Dictionary<string, string>
        {
            ["client_key"] = RequireClientKey(),
            ["client_secret"] = RequireClientSecret(),
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirectUri
        };
        return SendFormAsync<TikTokOAuthResponse>(HttpMethod.Post, "v2/oauth/token/", body, null, ct);
    }

    public Task<TikTokOAuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var body = new Dictionary<string, string>
        {
            ["client_key"] = RequireClientKey(),
            ["client_secret"] = RequireClientSecret(),
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };
        return SendFormAsync<TikTokOAuthResponse>(HttpMethod.Post, "v2/oauth/token/", body, null, ct);
    }

    public Task<TikTokDataWrapper<TikTokInitResponse>> InitVideoPublishAsync(string videoUrl, string caption, string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new
        {
            post_info = new
            {
                title = caption,
                privacy_level = "PUBLIC_TO_EVERYONE",
                disable_comment = false,
                disable_duet = false,
                disable_stitch = false,
                video_cover_timestamp_ms = 1000
            },
            source_info = new
            {
                source = "FILE_UPLOAD",
                video_url = videoUrl,
                video_size = 0
            }
        });
        return SendJsonAsync<TikTokDataWrapper<TikTokInitResponse>>(HttpMethod.Post, "v2/post/publish/inbox/video/init/", payload, accessToken, ct);
    }

    public Task<TikTokDataWrapper<TikTokPublishStatusResponse>> GetPublishStatusAsync(string publishId, string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { publish_id = publishId });
        return SendJsonAsync<TikTokDataWrapper<TikTokPublishStatusResponse>>(HttpMethod.Post, "v2/post/publish/status/fetch/", payload, accessToken, ct);
    }

    public Task<TikTokDataWrapper<TikTokUserInfoResponse>> GetUserInfoAsync(string accessToken, CancellationToken ct)
        => SendJsonAsync<TikTokDataWrapper<TikTokUserInfoResponse>>(HttpMethod.Get, "v2/user/info/", null, accessToken, ct);

    public Task<TikTokDataWrapper<TikTokVideoListResponse>> GetVideoListAsync(string accessToken, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new { max_count = 20 });
        return SendJsonAsync<TikTokDataWrapper<TikTokVideoListResponse>>(HttpMethod.Post, "v2/video/list/", payload, accessToken, ct);
    }

    public Task<TikTokDataWrapper<TikTokInsightsResponse>> GetVideoInsightsAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<TikTokDataWrapper<TikTokInsightsResponse>>(HttpMethod.Get, $"v2/video/query/?fields=id,view_count,like_count,comment_count,share_count&video_ids={Uri.EscapeDataString(videoId)}", null, accessToken, ct);

    public Task DeleteVideoAsync(string videoId, string accessToken, CancellationToken ct)
        => SendJsonAsync<JsonElement>(HttpMethod.Delete, $"v2/video/delete/?video_id={Uri.EscapeDataString(videoId)}", null, accessToken, ct);

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string endpoint, string? jsonPayload, string? accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        if (jsonPayload is not null)
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw ParseError(body, response.StatusCode);

        // unwrap TikTok error wrapper
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(TikTokDataWrapper<>))
        {
            var wrapper = JsonSerializer.Deserialize<T>(body, JsonOptions);
            if (wrapper is null) throw new TikTokApiException("Empty response", statusCode: response.StatusCode);
            // check error field inside wrapper via reflection JSON element? Instead deserialize to JsonElement first
            var element = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
            if (element.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
            {
                if (err.TryGetProperty("code", out var code) && code.GetString() != "ok" && code.GetString() != null)
                {
                    var msg = err.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "TikTok API error";
                    throw new TikTokApiException(msg ?? "TikTok API error", statusCode: response.StatusCode);
                }
            }
            return wrapper;
        }

        try { return JsonSerializer.Deserialize<T>(body, JsonOptions) ?? throw new TikTokApiException("Empty body", statusCode: response.StatusCode); }
        catch (JsonException ex) { throw new TikTokApiException("Invalid JSON", statusCode: response.StatusCode, inner: ex); }
    }

    private async Task<T> SendFormAsync<T>(HttpMethod method, string endpoint, IReadOnlyDictionary<string,string> form, string? accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        request.Content = new FormUrlEncodedContent(form);
        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw ParseError(body, response.StatusCode);
        try { return JsonSerializer.Deserialize<T>(body, JsonOptions) ?? throw new TikTokApiException("Empty body", statusCode: response.StatusCode); }
        catch (JsonException ex) { throw new TikTokApiException("Invalid JSON", statusCode: response.StatusCode, inner: ex); }
    }

    private static TikTokApiException ParseError(string body, HttpStatusCode status)
    {
        try
        {
            var err = JsonSerializer.Deserialize<TikTokErrorResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (err?.Error is not null)
                return new TikTokApiException($"{err.Error}: {err.ErrorDescription}", statusCode: status, logId: err.LogId);
        }
        catch { }
        return new TikTokApiException($"TikTok API returned HTTP {(int)status} ({status}). Body: {body[..Math.Min(200, body.Length)]}", statusCode: status);
    }

    private string RequireClientKey() => string.IsNullOrWhiteSpace(options.ClientKey) ? throw new InvalidOperationException($"'{TikTokProviderOptions.SectionName}:ClientKey' is not configured.") : options.ClientKey;
    private string RequireClientSecret() => string.IsNullOrWhiteSpace(options.ClientSecret) ? throw new InvalidOperationException($"'{TikTokProviderOptions.SectionName}:ClientSecret' is not configured.") : options.ClientSecret;
}
