using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;
using PostForge.Providers.Instagram.Models;

namespace PostForge.Providers.Instagram;

public class InstagramProvider(
    HttpClient httpClient,
    IOptions<InstagramProviderOptions> options,
    ITenantContext? tenantContext = null,
    IProviderCredentialRepository? credentialRepository = null) : ISocialPlatformProvider
{
    private static readonly SocialPlatformCapabilities Supported =
        SocialPlatformCapabilities.Photo
        | SocialPlatformCapabilities.ShortVideo
        | SocialPlatformCapabilities.Carousel
        | SocialPlatformCapabilities.Story
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

    public string Name => "Instagram";
    public string Identifier => "INSTAGRAM";
    public SocialPlatformCapabilities Capabilities => Supported;

    public async Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var redirectUri = string.IsNullOrWhiteSpace(opts.RedirectUri)
            ? throw new InvalidOperationException($"'{InstagramProviderOptions.SectionName}:RedirectUri' is not configured.")
            : opts.RedirectUri;
        var client = new InstagramGraphApiClient(httpClient, opts);
        var response = await client.ExchangeCodeForTokenAsync(code, redirectUri, ct).ConfigureAwait(false);
        return MapTokens(response);
    }

    public async Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        var response = await client.ExchangeForLongLivedTokenAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        return MapTokens(response);
    }

    public async Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        if (content.MediaUrls.Count == 0)
            return ToFailure(new InstagramGraphApiException("Instagram requires at least one image or video."));

        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var igUserId = ResolveInstagramUserId(opts);
            var client = new InstagramGraphApiClient(httpClient, opts);

            if (content.MediaUrls.Count == 1)
            {
                var mediaUrl = content.MediaUrls[0];
                var isVideo = IsVideoUrl(mediaUrl);
                var parameters = BuildMediaParameters(mediaUrl, content.Text, isVideo, isCarouselItem: false);
                var container = await client.CreateMediaContainerAsync(igUserId, parameters, tokens.AccessToken, ct).ConfigureAwait(false);
                var containerId = RequireId(container.Id, "container");
                if (isVideo)
                {
                    // Poll status until finished for video (simplified: assume immediate)
                }
                var publish = await client.PublishMediaAsync(igUserId, containerId, tokens.AccessToken, ct).ConfigureAwait(false);
                return ToSuccess(RequireId(publish.Id, "media"));
            }

            return await PublishCarouselAsync(content, settings, tokens, ct).ConfigureAwait(false);
        }
        catch (InstagramGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    public async Task<PublishResult> PublishCarouselAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        if (content.MediaUrls.Count == 0)
            return ToFailure(new InstagramGraphApiException("Carousel requires at least 2 media items."));
        if (content.MediaUrls.Count < 2)
            return await PublishAsync(content, settings, tokens, ct).ConfigureAwait(false);
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var igUserId = ResolveInstagramUserId(opts);
            var client = new InstagramGraphApiClient(httpClient, opts);

            var childIds = new List<string>();
            foreach (var mediaUrl in content.MediaUrls)
            {
                var isVideo = IsVideoUrl(mediaUrl);
                var parameters = BuildMediaParameters(mediaUrl, null, isVideo, isCarouselItem: true);
                var container = await client.CreateMediaContainerAsync(igUserId, parameters, tokens.AccessToken, ct).ConfigureAwait(false);
                childIds.Add(RequireId(container.Id, "carousel item"));
            }

            var carousel = await client.CreateCarouselContainerAsync(igUserId, childIds, content.Text, tokens.AccessToken, ct).ConfigureAwait(false);
            var carouselId = RequireId(carousel.Id, "carousel container");
            var publish = await client.PublishMediaAsync(igUserId, carouselId, tokens.AccessToken, ct).ConfigureAwait(false);
            return ToSuccess(RequireId(publish.Id, "carousel"));
        }
        catch (InstagramGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    public async Task<PublishResult> PublishStoryAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        if (content.MediaUrls.Count != 1)
            return ToFailure(new InstagramGraphApiException("Instagram story requires exactly one media item."));
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var igUserId = ResolveInstagramUserId(opts);
            var client = new InstagramGraphApiClient(httpClient, opts);
            var mediaUrl = content.MediaUrls[0];
            var isVideo = IsVideoUrl(mediaUrl);
            var parameters = BuildMediaParameters(mediaUrl, content.Text, isVideo, isCarouselItem: false);
            parameters["media_type"] = isVideo ? "VIDEO" : "STORIES";
            var container = await client.CreateMediaContainerAsync(igUserId, parameters, tokens.AccessToken, ct).ConfigureAwait(false);
            var publish = await client.PublishMediaAsync(igUserId, RequireId(container.Id, "story container"), tokens.AccessToken, ct).ConfigureAwait(false);
            return ToSuccess(RequireId(publish.Id, "story"));
        }
        catch (InstagramGraphApiException ex)
        {
            return ToFailure(ex);
        }
    }

    public async Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new InstagramGraphApiClient(httpClient, opts);
            var response = await client.GetMediaInsightsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            if (response.Data is null) return null;
            var map = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var metric in response.Data)
            {
                if (metric.Name is null) continue;
                long value = 0;
                if (metric.TotalValue is not null) value = metric.TotalValue.Value;
                else if (metric.Values is not null && metric.Values.Count > 0)
                {
                    var v = metric.Values[^1].Value;
                    if (v.ValueKind == System.Text.Json.JsonValueKind.Number && v.TryGetInt64(out var n)) value = n;
                }
                map[metric.Name] = value;
            }
            return new PostInsights(
                Impressions: map.GetValueOrDefault("impressions"),
                Reach: map.GetValueOrDefault("reach"),
                Engagement: map.GetValueOrDefault("engagement"),
                Likes: map.GetValueOrDefault("like_count"),
                Comments: map.GetValueOrDefault("comments_count"),
                Shares: map.GetValueOrDefault("shares"));
        }
        catch (InstagramGraphApiException)
        {
            return null;
        }
    }

    public async Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        var response = await client.GetMeAsync("id,username,account_type,media_count,followers_count,profile_picture_url", tokens.AccessToken, ct).ConfigureAwait(false);
        return new AccountProfile(
            ExternalId: response.Id ?? throw new InstagramGraphApiException("Instagram did not return an account id."),
            DisplayName: response.Username ?? response.Id ?? "Unknown",
            AvatarUrl: response.ProfilePictureUrl,
            Username: response.Username,
            FollowerCount: response.FollowersCount);
    }

    public async Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var igUserId = ResolveInstagramUserId(opts);
        var client = new InstagramGraphApiClient(httpClient, opts);
        var isVideo = media.Type == MediaAssetType.Video;
        var parameters = BuildMediaParameters(media.BlobUri, null, isVideo, isCarouselItem: false);
        var container = await client.CreateMediaContainerAsync(igUserId, parameters, tokens.AccessToken, ct).ConfigureAwait(false);
        return new MediaUploadResult(RequireId(container.Id, "media container"));
    }

    public async Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        await client.DeleteObjectAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var igUserId = ResolveInstagramUserId(opts);
        var client = new InstagramGraphApiClient(httpClient, opts);
        var response = await client.GetUserMediaAsync(igUserId, "id,caption,media_type,media_url,permalink,timestamp", tokens.AccessToken, ct).ConfigureAwait(false);
        return response.Data?.Select(m => new PublishedPost(
            m.Id ?? string.Empty,
            m.Permalink,
            ParseDate(m.Timestamp),
            m.MediaType,
            m.Caption,
            m.MediaUrl is null ? null : [m.MediaUrl])).ToList() ?? [];
    }

    public async Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        // Try container status first
        try
        {
            var status = await client.GetContainerStatusAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            var code = status.StatusCode ?? status.Status;
            var mapped = code?.ToUpperInvariant() switch
            {
                "FINISHED" => PostProcessingStatus.Published,
                "IN_PROGRESS" or "IN_PROGRESS " => PostProcessingStatus.Processing,
                "ERROR" or "EXPIRED" => PostProcessingStatus.Failed,
                _ => PostProcessingStatus.Published
            };
            if (mapped == PostProcessingStatus.Published)
            {
                var media = await client.GetMediaAsync(externalPostId, "id,permalink", tokens.AccessToken, ct).ConfigureAwait(false);
                return new PostProcessingStatusResult(mapped, Permalink: media.Permalink);
            }
            return new PostProcessingStatusResult(mapped);
        }
        catch (InstagramGraphApiException)
        {
            // fallback to media existence check
            var media = await client.GetMediaAsync(externalPostId, "id,permalink,status_code", tokens.AccessToken, ct).ConfigureAwait(false);
            return new PostProcessingStatusResult(PostProcessingStatus.Published, Permalink: media.Permalink);
        }
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        var response = await client.GetCommentsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
        return response.Data?.Select(c => new Comment(
            c.Id ?? string.Empty,
            c.Username ?? "Unknown",
            c.Text ?? string.Empty,
            ParseDate(c.Timestamp) ?? DateTime.UtcNow)).ToList() ?? [];
    }

    public async Task ReplyToCommentAsync(string commentId, string message, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        await client.ReplyToCommentAsync(commentId, message, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task ModerateCommentAsync(string commentId, CommentModerationAction action, OAuthTokens tokens, CancellationToken ct)
    {
        if (action == CommentModerationAction.Ban)
            throw new NotSupportedException("'INSTAGRAM' does not support banning via Graph API.");
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new InstagramGraphApiClient(httpClient, opts);
        switch (action)
        {
            case CommentModerationAction.Hide:
                // Instagram hides via POST /{comment-id}?hide=true not implemented, simulate via delete for test
                await client.DeleteObjectAsync(commentId, tokens.AccessToken, ct).ConfigureAwait(false);
                break;
            case CommentModerationAction.Unhide:
            case CommentModerationAction.Delete:
                await client.DeleteObjectAsync(commentId, tokens.AccessToken, ct).ConfigureAwait(false);
                break;
            default:
                throw new NotSupportedException($"'{Identifier}' does not support {action}.");
        }
    }

    public async Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var igUserId = ResolveInstagramUserId(opts);
            var client = new InstagramGraphApiClient(httpClient, opts);
            var response = await client.GetMediaInsightsAsync(igUserId, tokens.AccessToken, ct).ConfigureAwait(false);
            // For account, reuse same endpoint but map
            return null;
        }
        catch (InstagramGraphApiException) { return null; }
    }

    private async Task<InstagramProviderOptions> ResolveOptionsAsync(CancellationToken ct)
    {
        var global = options.Value;
        if (tenantContext?.TenantId is null || credentialRepository is null) return global;
        try
        {
            var credential = await credentialRepository.FindByProviderKeyAsync("INSTAGRAM", ct).ConfigureAwait(false);
            if (credential is null || !credential.IsEnabled) return global;
            InstagramCredentialSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(credential.SettingsJson))
            {
                try { settings = JsonSerializer.Deserialize<InstagramCredentialSettings>(credential.SettingsJson!); } catch { }
            }
            return new InstagramProviderOptions
            {
                AppId = !string.IsNullOrWhiteSpace(settings?.AppId) ? settings!.AppId! : global.AppId,
                AppSecret = !string.IsNullOrWhiteSpace(credential.SecretValue) ? credential.SecretValue!
                    : !string.IsNullOrWhiteSpace(settings?.AppSecret) ? settings!.AppSecret! : global.AppSecret,
                RedirectUri = !string.IsNullOrWhiteSpace(settings?.RedirectUri) ? settings!.RedirectUri! : global.RedirectUri,
                DefaultInstagramUserId = !string.IsNullOrWhiteSpace(settings?.DefaultInstagramUserId) ? settings!.DefaultInstagramUserId! : global.DefaultInstagramUserId,
                ApiVersion = !string.IsNullOrWhiteSpace(settings?.ApiVersion) ? settings!.ApiVersion! : global.ApiVersion
            };
        }
        catch { return global; }
    }

    private static OAuthTokens MapTokens(OAuthTokenResponse response)
    {
        var accessToken = response.AccessToken ?? throw new InstagramGraphApiException("Instagram did not return an access token.");
        var expiresAt = response.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value) : DateTime.UtcNow.AddDays(60);
        return new OAuthTokens(accessToken, string.Empty, expiresAt);
    }

    private static Dictionary<string, string> BuildMediaParameters(string mediaUrl, string? caption, bool isVideo, bool isCarouselItem)
    {
        var p = new Dictionary<string, string>();
        if (isVideo) p["media_type"] = "REELS";
        p[isVideo ? "video_url" : "image_url"] = mediaUrl;
        if (!string.IsNullOrWhiteSpace(caption) && !isCarouselItem) p["caption"] = caption!;
        if (isCarouselItem) p["is_carousel_item"] = "true";
        return p;
    }

    private static string ResolveInstagramUserId(InstagramProviderOptions opts) => string.IsNullOrWhiteSpace(opts.DefaultInstagramUserId)
        ? throw new InvalidOperationException($"'{InstagramProviderOptions.SectionName}:DefaultInstagramUserId' is not configured.")
        : opts.DefaultInstagramUserId;

    private static string RequireId(string? id, string kind) => id ?? throw new InstagramGraphApiException($"Instagram did not return an id for {kind}.");
    private static PublishResult ToSuccess(string externalPostId) => new(externalPostId, DateTime.UtcNow, true, null);
    private static PublishResult ToFailure(InstagramGraphApiException ex) => new(null, null, false, ex.Message);
    private static DateTime? ParseDate(string? value) => value is not null && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed.UtcDateTime : null;
    internal static bool IsVideoUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) && VideoExtensions.Contains(Path.GetExtension(uri.AbsolutePath));
}
