using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PostForge.Providers.Facebook.Models;

namespace PostForge.Providers.Facebook;

internal sealed class FacebookGraphApiClient(HttpClient http, FacebookProviderOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string InsightsMetrics =
        "post_impressions,post_impressions_unique,post_engaged_users,post_reactions_like_total,post_comments,post_shares";

    private const string PageInsightsMetrics =
        "page_fans,page_profile_views,page_impressions,page_impressions_unique,page_engaged_users";

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken ct)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = RequireAppId(),
            ["client_secret"] = RequireAppSecret(),
            ["redirect_uri"] = redirectUri,
            ["code"] = code
        };

        return await SendAsync<OAuthTokenResponse>(HttpMethod.Get, "oauth/access_token", query, null, null, ct);
    }

    public async Task<OAuthTokenResponse> ExchangeForLongLivedTokenAsync(string shortLivedToken, CancellationToken ct)
    {
        var query = new Dictionary<string, string>
        {
            ["grant_type"] = "fb_exchange_token",
            ["client_id"] = RequireAppId(),
            ["client_secret"] = RequireAppSecret(),
            ["fb_exchange_token"] = shortLivedToken
        };

        return await SendAsync<OAuthTokenResponse>(HttpMethod.Get, "oauth/access_token", query, null, null, ct);
    }

    public Task<FacebookIdResponse> PublishFeedPostAsync(
        string pageId,
        Dictionary<string, string> parameters,
        string accessToken,
        CancellationToken ct)
        => SendAsync<FacebookIdResponse>(HttpMethod.Post, $"{pageId}/feed", null, parameters, accessToken, ct);

    public Task<FacebookIdResponse> UploadPhotoAsync(
        string pageId,
        string photoUrl,
        bool published,
        bool temporary,
        string? message,
        string accessToken,
        CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["url"] = photoUrl,
            ["published"] = published ? "true" : "false",
            ["access_token"] = accessToken
        };

        if (temporary)
            form["temporary"] = "true";
        if (message is not null)
            form["message"] = message;

        return SendAsync<FacebookIdResponse>(HttpMethod.Post, $"{pageId}/photos", null, form, accessToken, ct);
    }

    public Task<FacebookIdResponse> PublishVideoAsync(
        string pageId,
        string fileUrl,
        string? description,
        bool published,
        long? scheduledPublishTime,
        string accessToken,
        CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["file_url"] = fileUrl,
            ["published"] = published ? "true" : "false",
            ["access_token"] = accessToken
        };

        if (description is not null)
            form["description"] = description;
        if (scheduledPublishTime.HasValue)
            form["scheduled_publish_time"] = scheduledPublishTime.Value.ToString(CultureInfo.InvariantCulture);

        return SendAsync<FacebookIdResponse>(HttpMethod.Post, $"{pageId}/videos", null, form, accessToken, ct);
    }

    public Task<FacebookInsightsResponse> GetPostInsightsAsync(string postId, string accessToken, CancellationToken ct)
        => SendAsync<FacebookInsightsResponse>(
            HttpMethod.Get,
            $"{postId}/insights",
            new Dictionary<string, string> { ["metric"] = InsightsMetrics, ["period"] = "lifetime" },
            null,
            accessToken,
            ct);

    public Task<FacebookInsightsResponse> GetPageInsightsAsync(string pageId, string accessToken, CancellationToken ct)
        => SendAsync<FacebookInsightsResponse>(
            HttpMethod.Get,
            $"{pageId}/insights",
            new Dictionary<string, string> { ["metric"] = PageInsightsMetrics, ["period"] = "day" },
            null,
            accessToken,
            ct);

    public Task<FacebookMeResponse> GetMeAsync(string fields, string accessToken, CancellationToken ct)
        => SendAsync<FacebookMeResponse>(
            HttpMethod.Get,
            "me",
            new Dictionary<string, string> { ["fields"] = fields },
            null,
            accessToken,
            ct);

    public Task<FacebookPostResponse> GetPostAsync(string postId, string fields, string accessToken, CancellationToken ct)
        => SendAsync<FacebookPostResponse>(
            HttpMethod.Get,
            postId,
            new Dictionary<string, string> { ["fields"] = fields },
            null,
            accessToken,
            ct);

    public Task<FacebookPostCollectionResponse> GetPagePostsAsync(string pageId, string fields, string accessToken, CancellationToken ct)
        => SendAsync<FacebookPostCollectionResponse>(
            HttpMethod.Get,
            $"{pageId}/posts",
            new Dictionary<string, string> { ["fields"] = fields, ["limit"] = "100" },
            null,
            accessToken,
            ct);

    public Task<FacebookIdResponse> UpdatePostAsync(string postId, string message, string accessToken, CancellationToken ct)
        => SendAsync<FacebookIdResponse>(
            HttpMethod.Post,
            postId,
            null,
            new Dictionary<string, string> { ["message"] = message, ["access_token"] = accessToken },
            accessToken,
            ct);

    public Task DeleteObjectAsync(string id, string accessToken, CancellationToken ct)
        => SendAsync<JsonElement>(
            HttpMethod.Delete,
            id,
            null,
            null,
            accessToken,
            ct);

    public Task<FacebookCommentCollectionResponse> GetCommentsAsync(string postId, string accessToken, CancellationToken ct)
        => SendAsync<FacebookCommentCollectionResponse>(
            HttpMethod.Get,
            $"{postId}/comments",
            new Dictionary<string, string> { ["fields"] = "id,from,message,created_time" },
            null,
            accessToken,
            ct);

    public Task<FacebookIdResponse> PostCommentAsync(string commentId, string message, string accessToken, CancellationToken ct)
        => SendAsync<FacebookIdResponse>(
            HttpMethod.Post,
            $"{commentId}/comments",
            null,
            new Dictionary<string, string> { ["message"] = message, ["access_token"] = accessToken },
            accessToken,
            ct);

    public Task<FacebookSuccessResponse> SetCommentHiddenAsync(string commentId, bool isHidden, string accessToken, CancellationToken ct)
        => SendAsync<FacebookSuccessResponse>(
            HttpMethod.Post,
            commentId,
            null,
            new Dictionary<string, string> { ["is_hidden"] = isHidden ? "true" : "false", ["access_token"] = accessToken },
            accessToken,
            ct);

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        IReadOnlyDictionary<string, string>? query,
        IReadOnlyDictionary<string, string>? form,
        string? accessToken,
        CancellationToken ct)
    {
        var hasBody = form is not null && form.Count > 0;

        var queryParams = query is null ? new Dictionary<string, string>() : new Dictionary<string, string>(query);
        var formParams = form is null ? new Dictionary<string, string>() : new Dictionary<string, string>(form);

        if (!string.IsNullOrEmpty(accessToken))
        {
            if (hasBody)
            {
                formParams["access_token"] = accessToken;
                if (options.EnableAppSecretProof)
                    formParams["appsecret_proof"] = ComputeAppSecretProof(accessToken);
            }
            else
            {
                queryParams["access_token"] = accessToken;
                if (options.EnableAppSecretProof)
                    queryParams["appsecret_proof"] = ComputeAppSecretProof(accessToken);
            }
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
                ?? throw new FacebookGraphApiException("Empty response body.", statusCode: response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new FacebookGraphApiException("Invalid JSON response from the Graph API.", statusCode: response.StatusCode, innerException: ex);
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

    private string ComputeAppSecretProof(string accessToken)
    {
        var secret = RequireAppSecret();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(accessToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static FacebookGraphApiException ParseError(string body, HttpStatusCode statusCode)
    {
        try
        {
            var response = JsonSerializer.Deserialize<FacebookErrorResponse>(body, JsonOptions);
            if (response?.Error is { Message: not null } error)
            {
                return new FacebookGraphApiException(
                    error.Message,
                    error.Type,
                    error.Code,
                    error.ErrorSubcode,
                    error.FbTraceId,
                    statusCode);
            }
        }
        catch (JsonException)
        {
            // Fall through to the generic error below.
        }

        return new FacebookGraphApiException($"Facebook Graph API returned HTTP {(int)statusCode} ({statusCode}).", statusCode: statusCode);
    }

    private string RequireAppId() => string.IsNullOrWhiteSpace(options.AppId)
        ? throw new InvalidOperationException($"'{FacebookProviderOptions.SectionName}:AppId' is not configured.")
        : options.AppId;

    private string RequireAppSecret() => string.IsNullOrWhiteSpace(options.AppSecret)
        ? throw new InvalidOperationException($"'{FacebookProviderOptions.SectionName}:AppSecret' is not configured.")
        : options.AppSecret;
}

