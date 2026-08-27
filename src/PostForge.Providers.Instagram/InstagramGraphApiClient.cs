using System.Net;
using System.Text.Json;
using PostForge.Providers.Instagram.Models;

namespace PostForge.Providers.Instagram;

internal sealed class InstagramGraphApiClient(HttpClient http, InstagramProviderOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken ct)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = RequireAppId(),
            ["client_secret"] = RequireAppSecret(),
            ["redirect_uri"] = redirectUri,
            ["code"] = code
        };
        return SendAsync<OAuthTokenResponse>(HttpMethod.Get, "oauth/access_token", query, null, null, ct);
    }

    public Task<OAuthTokenResponse> ExchangeForLongLivedTokenAsync(string shortLivedToken, CancellationToken ct)
    {
        var query = new Dictionary<string, string>
        {
            ["grant_type"] = "fb_exchange_token",
            ["client_id"] = RequireAppId(),
            ["client_secret"] = RequireAppSecret(),
            ["fb_exchange_token"] = shortLivedToken
        };
        return SendAsync<OAuthTokenResponse>(HttpMethod.Get, "oauth/access_token", query, null, null, ct);
    }

    public Task<InstagramIdResponse> CreateMediaContainerAsync(string igUserId, Dictionary<string, string> parameters, string accessToken, CancellationToken ct)
        => SendAsync<InstagramIdResponse>(HttpMethod.Post, $"{igUserId}/media", null, parameters, accessToken, ct);

    public Task<InstagramIdResponse> PublishMediaAsync(string igUserId, string creationId, string accessToken, CancellationToken ct)
        => SendAsync<InstagramIdResponse>(HttpMethod.Post, $"{igUserId}/media_publish", null, new Dictionary<string, string> { ["creation_id"] = creationId, ["access_token"] = accessToken }, accessToken, ct);

    public Task<InstagramIdResponse> CreateCarouselContainerAsync(string igUserId, List<string> childIds, string? caption, string accessToken, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["media_type"] = "CAROUSEL",
            ["caption"] = caption ?? string.Empty,
            ["children"] = string.Join(",", childIds),
            ["access_token"] = accessToken
        };
        return SendAsync<InstagramIdResponse>(HttpMethod.Post, $"{igUserId}/media", null, form, accessToken, ct);
    }

    public Task<InstagramMediaResponse> GetMediaAsync(string mediaId, string fields, string accessToken, CancellationToken ct)
        => SendAsync<InstagramMediaResponse>(HttpMethod.Get, mediaId, new Dictionary<string, string> { ["fields"] = fields }, null, accessToken, ct);

    public Task<InstagramContainerStatusResponse> GetContainerStatusAsync(string containerId, string accessToken, CancellationToken ct)
        => SendAsync<InstagramContainerStatusResponse>(HttpMethod.Get, containerId, new Dictionary<string, string> { ["fields"] = "status_code,status" }, null, accessToken, ct);

    public Task<InstagramMediaCollectionResponse> GetUserMediaAsync(string igUserId, string fields, string accessToken, CancellationToken ct)
        => SendAsync<InstagramMediaCollectionResponse>(HttpMethod.Get, $"{igUserId}/media", new Dictionary<string, string> { ["fields"] = fields, ["limit"] = "50" }, null, accessToken, ct);

    public Task<InstagramUserResponse> GetMeAsync(string fields, string accessToken, CancellationToken ct)
        => SendAsync<InstagramUserResponse>(HttpMethod.Get, "me", new Dictionary<string, string> { ["fields"] = fields }, null, accessToken, ct);

    public Task<InstagramInsightsResponse> GetMediaInsightsAsync(string mediaId, string accessToken, CancellationToken ct)
        => SendAsync<InstagramInsightsResponse>(HttpMethod.Get, $"{mediaId}/insights", new Dictionary<string, string> { ["metric"] = "engagement,impressions,reach,saved", ["period"] = "lifetime" }, null, accessToken, ct);

    public Task<InstagramCommentCollectionResponse> GetCommentsAsync(string mediaId, string accessToken, CancellationToken ct)
        => SendAsync<InstagramCommentCollectionResponse>(HttpMethod.Get, $"{mediaId}/comments", new Dictionary<string, string> { ["fields"] = "id,text,timestamp,username" }, null, accessToken, ct);

    public Task<InstagramIdResponse> ReplyToCommentAsync(string commentId, string message, string accessToken, CancellationToken ct)
        => SendAsync<InstagramIdResponse>(HttpMethod.Post, $"{commentId}/replies", null, new Dictionary<string, string> { ["message"] = message, ["access_token"] = accessToken }, accessToken, ct);

    public Task DeleteObjectAsync(string id, string accessToken, CancellationToken ct)
        => SendAsync<JsonElement>(HttpMethod.Delete, id, null, null, accessToken, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string endpoint, IReadOnlyDictionary<string, string>? query, IReadOnlyDictionary<string, string>? form, string? accessToken, CancellationToken ct)
    {
        var hasBody = form is not null && form.Count > 0;
        var queryParams = query is null ? new Dictionary<string, string>() : new Dictionary<string, string>(query);
        var formParams = form is null ? new Dictionary<string, string>() : new Dictionary<string, string>(form);

        if (!string.IsNullOrEmpty(accessToken))
        {
            if (hasBody)
                formParams.TryAdd("access_token", accessToken);
            else
                queryParams.TryAdd("access_token", accessToken);
        }

        var url = BuildUrl(endpoint, queryParams);

        using var request = new HttpRequestMessage(method, url);
        if (hasBody)
            request.Content = new FormUrlEncodedContent(formParams);

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw ParseError(body, response.StatusCode);

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions)
                ?? throw new InstagramGraphApiException("Empty response body.", statusCode: response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new InstagramGraphApiException("Invalid JSON response from Instagram Graph API.", statusCode: response.StatusCode, innerException: ex);
        }
    }

    private string BuildUrl(string endpoint, IReadOnlyDictionary<string, string> queryParams)
    {
        var version = NormalizeApiVersion(options.ApiVersion);
        var path = queryParams.Count == 0
            ? $"{version}/{endpoint}"
            : $"{version}/{endpoint}?{string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"))}";
        return path;
    }

    private static string NormalizeApiVersion(string apiVersion)
    {
        var version = apiVersion.Trim().TrimStart('/');
        return version.StartsWith('v') ? version : $"v{version}";
    }

    private static InstagramGraphApiException ParseError(string body, HttpStatusCode statusCode)
    {
        try
        {
            var response = JsonSerializer.Deserialize<InstagramErrorResponse>(body, JsonOptions);
            if (response?.Error is { Message: not null } error)
                return new InstagramGraphApiException(error.Message, error.Type, error.Code, error.ErrorSubcode, error.FbTraceId, statusCode);
        }
        catch (JsonException) { }
        return new InstagramGraphApiException($"Instagram Graph API returned HTTP {(int)statusCode} ({statusCode}).", statusCode: statusCode);
    }

    private string RequireAppId() => string.IsNullOrWhiteSpace(options.AppId)
        ? throw new InvalidOperationException($"'{InstagramProviderOptions.SectionName}:AppId' is not configured.")
        : options.AppId;

    private string RequireAppSecret() => string.IsNullOrWhiteSpace(options.AppSecret)
        ? throw new InvalidOperationException($"'{InstagramProviderOptions.SectionName}:AppSecret' is not configured.")
        : options.AppSecret;
}
