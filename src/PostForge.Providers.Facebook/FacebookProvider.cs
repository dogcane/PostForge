using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;
using PostForge.Providers.Facebook.Models;

namespace PostForge.Providers.Facebook;

public class FacebookProvider : ISocialPlatformProvider
{
    private static readonly SocialPlatformCapabilities Supported =
        SocialPlatformCapabilities.TextOnly
        | SocialPlatformCapabilities.Photo
        | SocialPlatformCapabilities.Video
        | SocialPlatformCapabilities.ShortVideo
        | SocialPlatformCapabilities.Carousel
        | SocialPlatformCapabilities.Story
        | SocialPlatformCapabilities.Live
        | SocialPlatformCapabilities.Link
        | SocialPlatformCapabilities.Hashtags
        | SocialPlatformCapabilities.MentionUsers
        | SocialPlatformCapabilities.UserTagWithCoordinates
        | SocialPlatformCapabilities.LocationTag
        | SocialPlatformCapabilities.AltText
        | SocialPlatformCapabilities.CustomThumbnail
        | SocialPlatformCapabilities.Collaborators
        | SocialPlatformCapabilities.PaidPartnership
        | SocialPlatformCapabilities.AiGeneratedLabel
        | SocialPlatformCapabilities.LicensedAudio
        | SocialPlatformCapabilities.CallToAction
        | SocialPlatformCapabilities.NativeScheduling
        | SocialPlatformCapabilities.CommentControls
        | SocialPlatformCapabilities.AudienceTargeting
        | SocialPlatformCapabilities.EditPost
        | SocialPlatformCapabilities.DeletePost
        | SocialPlatformCapabilities.ReadUserPosts
        | SocialPlatformCapabilities.PostStatusTracking
        | SocialPlatformCapabilities.MediaUploadApi
        | SocialPlatformCapabilities.ReadComments
        | SocialPlatformCapabilities.ReplyToComments
        | SocialPlatformCapabilities.ModerateComments
        | SocialPlatformCapabilities.ReadMentions
        | SocialPlatformCapabilities.DirectMessaging
        | SocialPlatformCapabilities.PostInsights
        | SocialPlatformCapabilities.AccountInsights
        | SocialPlatformCapabilities.AudienceInsights;

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".wmv", ".mpg", ".mpeg", ".webm", ".flv", ".m4v", ".mkv", ".3gp", ".3g2", ".ogv"
    };

    private readonly HttpClient _httpClient;
    private readonly IOptions<FacebookProviderOptions> _globalOptions;
    private readonly ITenantContext? _tenantContext;
    private readonly IProviderCredentialRepository? _credentialRepository;

    public FacebookProvider(HttpClient httpClient, IOptions<FacebookProviderOptions> options)
        : this(httpClient, options, null, null)
    {
    }

    public FacebookProvider(
        HttpClient httpClient,
        IOptions<FacebookProviderOptions> options,
        ITenantContext? tenantContext,
        IProviderCredentialRepository? credentialRepository)
    {
        _httpClient = httpClient;
        _globalOptions = options;
        _tenantContext = tenantContext;
        _credentialRepository = credentialRepository;
    }

    public string Name => "Facebook";
    public string Identifier => "FACEBOOK";
    public SocialPlatformCapabilities Capabilities => Supported;

    // ---- Core (OAuth + publishing + insights) ----

    public async Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var redirectUri = string.IsNullOrWhiteSpace(options.RedirectUri)
            ? throw new InvalidOperationException($"'{FacebookProviderOptions.SectionName}:RedirectUri' is not configured.")
            : options.RedirectUri;

        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client.ExchangeCodeForTokenAsync(code, redirectUri, ct).ConfigureAwait(false);
        return MapTokens(response);
    }

    public async Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client.ExchangeForLongLivedTokenAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        return MapTokens(response);
    }

    public async Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var pageId = ResolvePageId(options);

        try
        {
            var client = new FacebookGraphApiClient(_httpClient, options);

            if (content.MediaUrls.Count == 0)
            {
                var response = await client
                    .PublishFeedPostAsync(pageId, BuildFeedParameters(content.Text, published: true, null), tokens.AccessToken, ct)
                    .ConfigureAwait(false);
                return ToSuccess(RequireId(response.Id, "post"));
            }

            if (content.MediaUrls.Count == 1 && IsVideoUrl(content.MediaUrls[0]))
            {
                var response = await client
                    .PublishVideoAsync(pageId, content.MediaUrls[0], content.Text, published: true, null, tokens.AccessToken, ct)
                    .ConfigureAwait(false);
                return ToSuccess(RequireId(response.Id, "video"));
            }

            return await PublishPhotosAsync(client, pageId, content, tokens, null, ct).ConfigureAwait(false);
        }
        catch (FacebookGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    public async Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new FacebookGraphApiClient(_httpClient, options);
            var response = await client.GetPostInsightsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            var metrics = ToInsightValues(response);

            return new PostInsights(
                Impressions: metrics.GetValueOrDefault("post_impressions"),
                Reach: metrics.GetValueOrDefault("post_impressions_unique"),
                Engagement: metrics.GetValueOrDefault("post_engaged_users"),
                Likes: metrics.GetValueOrDefault("post_reactions_like_total"),
                Comments: metrics.GetValueOrDefault("post_comments"),
                Shares: metrics.GetValueOrDefault("post_shares"));
        }
        catch (FacebookGraphApiException)
        {
            return null;
        }
    }

    // ---- Account ----

    public async Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client
            .GetMeAsync("id,name,username,fan_count,picture.type(large)", tokens.AccessToken, ct)
            .ConfigureAwait(false);

        return new AccountProfile(
            ExternalId: response.Id ?? throw new FacebookGraphApiException("Facebook did not return an account id."),
            DisplayName: response.Name ?? response.Id ?? "Unknown",
            AvatarUrl: response.Picture?.Data?.Url,
            Username: response.Username,
            FollowerCount: response.FanCount);
    }

    // ---- Publishing extensions ----

    public async Task<PublishResult> PublishCarouselAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var pageId = ResolvePageId(options);
        try
        {
            var client = new FacebookGraphApiClient(_httpClient, options);
            return await PublishPhotosAsync(client, pageId, content, tokens, null, ct).ConfigureAwait(false);
        }
        catch (FacebookGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    public async Task<PublishResult> ScheduleAsync(
        PostContent content,
        PublishSettings settings,
        DateTime scheduledAtUtc,
        OAuthTokens tokens,
        CancellationToken ct)
    {
        var unixTime = new DateTimeOffset(scheduledAtUtc.ToUniversalTime()).ToUnixTimeSeconds();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (unixTime < now + 10 * 60)
            return ToFailure(new FacebookGraphApiException("scheduled_publish_time must be at least 10 minutes in the future."));
        if (unixTime > now + 30 * 24 * 60 * 60)
            return ToFailure(new FacebookGraphApiException("scheduled_publish_time must be within 30 days."));

        try
        {
            var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new FacebookGraphApiClient(_httpClient, options);
            var pageId = ResolvePageId(options);

            if (content.MediaUrls.Count == 0)
            {
                var response = await client
                    .PublishFeedPostAsync(pageId, BuildFeedParameters(content.Text, published: false, unixTime), tokens.AccessToken, ct)
                    .ConfigureAwait(false);
                return ToSuccess(RequireId(response.Id, "scheduled post"));
            }

            if (content.MediaUrls.Count == 1 && IsVideoUrl(content.MediaUrls[0]))
            {
                var response = await client
                    .PublishVideoAsync(pageId, content.MediaUrls[0], content.Text, published: false, unixTime, tokens.AccessToken, ct)
                    .ConfigureAwait(false);
                return ToSuccess(RequireId(response.Id, "scheduled video"));
            }

            return await PublishPhotosAsync(client, pageId, content, tokens, unixTime, ct).ConfigureAwait(false);
        }
        catch (FacebookGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    // ---- Media ----

    public async Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var pageId = ResolvePageId(options);

        if (media.Type == MediaAssetType.Video)
        {
            var response = await client
                .PublishVideoAsync(pageId, media.BlobUri, null, published: false, null, tokens.AccessToken, ct)
                .ConfigureAwait(false);
            return new MediaUploadResult(RequireId(response.Id, "video"));
        }

        var photo = await client
            .UploadPhotoAsync(pageId, media.BlobUri, published: false, temporary: false, message: null, tokens.AccessToken, ct)
            .ConfigureAwait(false);
        return new MediaUploadResult(RequireId(photo.Id, "photo"));
    }

    // ---- Post management ----

    public async Task<PublishedPost> UpdatePostAsync(
        string externalPostId,
        PostContent content,
        PublishSettings settings,
        OAuthTokens tokens,
        CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        await client.UpdatePostAsync(externalPostId, content.Text, tokens.AccessToken, ct).ConfigureAwait(false);
        var updated = await client
            .GetPostAsync(externalPostId, "id,message,created_time,permalink_url", tokens.AccessToken, ct)
            .ConfigureAwait(false);

        return new PublishedPost(
            updated.Id ?? externalPostId,
            updated.PermalinkUrl,
            ParseDate(updated.CreatedTime),
            Status: null,
            Caption: updated.Message ?? content.Text);
    }

    public async Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        await client.DeleteObjectAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client
            .GetPagePostsAsync(ResolvePageId(options), "id,message,created_time,permalink_url,full_picture", tokens.AccessToken, ct)
            .ConfigureAwait(false);

        return response.Data?
            .Select(post => new PublishedPost(
                post.Id ?? string.Empty,
                post.PermalinkUrl,
                ParseDate(post.CreatedTime),
                post.StatusType,
                post.Message,
                post.FullPicture is null ? null : [post.FullPicture]))
            .ToList()
            ?? [];
    }

    public async Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client
            .GetPostAsync(externalPostId, "is_published,permalink_url,status_type", tokens.AccessToken, ct)
            .ConfigureAwait(false);

        var status = response.IsPublished == true ? PostProcessingStatus.Published : PostProcessingStatus.Processing;
        return new PostProcessingStatusResult(status, ErrorMessage: null, response.PermalinkUrl);
    }

    // ---- Engagement ----

    public async Task<IReadOnlyList<Comment>> GetCommentsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        var response = await client.GetCommentsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);

        return response.Data?
            .Select(comment => new Comment(
                comment.Id ?? string.Empty,
                comment.From?.Name ?? comment.From?.Id ?? "Unknown",
                comment.Message ?? string.Empty,
                ParseDate(comment.CreatedTime) ?? DateTime.UtcNow,
                ReplyToExternalId: null))
            .ToList()
            ?? [];
    }

    public async Task ReplyToCommentAsync(string commentId, string message, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        await client.PostCommentAsync(commentId, message, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task ModerateCommentAsync(string commentId, CommentModerationAction action, OAuthTokens tokens, CancellationToken ct)
    {
        var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new FacebookGraphApiClient(_httpClient, options);
        switch (action)
        {
            case CommentModerationAction.Hide:
                await client.SetCommentHiddenAsync(commentId, isHidden: true, tokens.AccessToken, ct).ConfigureAwait(false);
                break;
            case CommentModerationAction.Unhide:
                await client.SetCommentHiddenAsync(commentId, isHidden: false, tokens.AccessToken, ct).ConfigureAwait(false);
                break;
            case CommentModerationAction.Delete:
                await client.DeleteObjectAsync(commentId, tokens.AccessToken, ct).ConfigureAwait(false);
                break;
            default:
                throw new NotSupportedException("'FACEBOOK' does not support banning comment authors via the Graph API.");
        }
    }

    // ---- Insights ----

    public async Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var options = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new FacebookGraphApiClient(_httpClient, options);
            var response = await client.GetPageInsightsAsync(ResolvePageId(options), tokens.AccessToken, ct).ConfigureAwait(false);
            var metrics = ToInsightValues(response);

            long? impressions = metrics.GetValueOrDefault("page_impressions");
            long? engaged = metrics.GetValueOrDefault("page_engaged_users");
            double? engagementRate = impressions > 0 && engaged.HasValue
                ? engaged.Value / (double)impressions.Value * 100
                : null;

            return new AccountInsights(
                FollowerCount: metrics.GetValueOrDefault("page_fans"),
                Impressions: impressions,
                Reach: metrics.GetValueOrDefault("page_impressions_unique"),
                ProfileViews: metrics.GetValueOrDefault("page_profile_views"),
                EngagementRate: engagementRate);
        }
        catch (FacebookGraphApiException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    // ---- Helpers + tenant resolution ----

    private async Task<FacebookProviderOptions> ResolveOptionsAsync(CancellationToken ct)
    {
        var global = _globalOptions.Value;

        if (_tenantContext?.TenantId is null || _credentialRepository is null)
            return global;

        try
        {
            var credential = await _credentialRepository.FindByProviderKeyAsync("FACEBOOK", ct).ConfigureAwait(false);
            if (credential is null || !credential.IsEnabled)
                return global;

            FacebookCredentialSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(credential.SettingsJson))
            {
                try
                {
                    settings = JsonSerializer.Deserialize<FacebookCredentialSettings>(credential.SettingsJson!);
                }
                catch
                {
                    // ignore malformed json, fallback to global
                }
            }

            var resolved = new FacebookProviderOptions
            {
                AppId = !string.IsNullOrWhiteSpace(settings?.AppId) ? settings!.AppId! : global.AppId,
                AppSecret = !string.IsNullOrWhiteSpace(credential.SecretValue) ? credential.SecretValue!
                    : !string.IsNullOrWhiteSpace(settings?.AppSecret) ? settings!.AppSecret!
                    : global.AppSecret,
                RedirectUri = !string.IsNullOrWhiteSpace(settings?.RedirectUri) ? settings!.RedirectUri! : global.RedirectUri,
                DefaultPageId = !string.IsNullOrWhiteSpace(settings?.DefaultPageId) ? settings!.DefaultPageId! : global.DefaultPageId,
                ApiVersion = !string.IsNullOrWhiteSpace(settings?.ApiVersion) ? settings!.ApiVersion! : global.ApiVersion,
                EnableAppSecretProof = settings?.EnableAppSecretProof ?? global.EnableAppSecretProof
            };

            // If resolved still empty for critical fields, fallback to global values already set
            return resolved;
        }
        catch
        {
            return global;
        }
    }

    private static OAuthTokens MapTokens(OAuthTokenResponse response)
    {
        var accessToken = response.AccessToken
            ?? throw new FacebookGraphApiException("Facebook did not return an access token.");
        var expiresAt = response.ExpiresIn.HasValue
            ? DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value)
            : DateTime.UtcNow.AddDays(60);

        return new OAuthTokens(accessToken, RefreshToken: string.Empty, expiresAt);
    }

    private async Task<PublishResult> PublishPhotosAsync(
        FacebookGraphApiClient client,
        string pageId,
        PostContent content,
        OAuthTokens tokens,
        long? scheduledPublishTime,
        CancellationToken ct)
    {
        var temporary = scheduledPublishTime.HasValue;
        var mediaIds = new List<string>();

        foreach (var mediaUrl in content.MediaUrls)
        {
            var upload = await client
                .UploadPhotoAsync(pageId, mediaUrl, published: false, temporary: temporary, message: null, tokens.AccessToken, ct)
                .ConfigureAwait(false);
            mediaIds.Add(RequireId(upload.Id, "photo"));
        }

        var attachedMedia = JsonSerializer.Serialize(mediaIds.Select(id => new Dictionary<string, string> { ["media_fbid"] = id }));
        var parameters = BuildFeedParameters(content.Text, published: !scheduledPublishTime.HasValue, scheduledPublishTime);
        parameters["attached_media"] = attachedMedia;

        var response = await client.PublishFeedPostAsync(pageId, parameters, tokens.AccessToken, ct).ConfigureAwait(false);
        return ToSuccess(RequireId(response.Id, "post"));
    }

    private static Dictionary<string, string> BuildFeedParameters(string message, bool published, long? scheduledPublishTime)
    {
        var parameters = new Dictionary<string, string>
        {
            ["message"] = message,
            ["published"] = published ? "true" : "false"
        };

        if (scheduledPublishTime.HasValue)
            parameters["scheduled_publish_time"] = scheduledPublishTime.Value.ToString(CultureInfo.InvariantCulture);

        return parameters;
    }

    private static Dictionary<string, long> ToInsightValues(FacebookInsightsResponse response)
    {
        var metrics = new Dictionary<string, long>(StringComparer.Ordinal);
        if (response.Data is null)
            return metrics;

        foreach (var result in response.Data)
        {
            if (result.Name is null || result.Values is null || result.Values.Count == 0)
                continue;

            var value = result.Values[^1].Value;
            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out var number))
                        metrics[result.Name] = number;
                    else if (value.TryGetDouble(out var numberDouble))
                        metrics[result.Name] = (long)numberDouble;
                    break;
                case JsonValueKind.Object:
                    long sum = 0;
                    foreach (var property in value.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out var item))
                            sum += item;
                    }

                    metrics[result.Name] = sum;
                    break;
            }
        }

        return metrics;
    }

    private static string ResolvePageId(FacebookProviderOptions options) => string.IsNullOrWhiteSpace(options.DefaultPageId)
        ? throw new InvalidOperationException($"'{FacebookProviderOptions.SectionName}:DefaultPageId' is not configured.")
        : options.DefaultPageId;

    private static string RequireId(string? id, string kind) => id
        ?? throw new FacebookGraphApiException($"Facebook did not return an id for the uploaded {kind}.");

    private static PublishResult ToSuccess(string externalPostId) => new(externalPostId, DateTime.UtcNow, IsSuccess: true, ErrorMessage: null);

    private static PublishResult ToFailure(FacebookGraphApiException exception) => new(null, null, IsSuccess: false, exception.Message);

    private static DateTime? ParseDate(string? value)
        => value is not null && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.UtcDateTime
            : null;

    internal static bool IsVideoUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) && VideoExtensions.Contains(Path.GetExtension(uri.AbsolutePath));
}
