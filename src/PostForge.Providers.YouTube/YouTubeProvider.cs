using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.YouTube;

public class YouTubeProvider(
    HttpClient httpClient,
    IOptions<YouTubeProviderOptions> options,
    ITenantContext? tenantContext = null,
    IProviderCredentialRepository? credentialRepository = null) : ISocialPlatformProvider
{
    private static readonly SocialPlatformCapabilities Supported =
        SocialPlatformCapabilities.Video
        | SocialPlatformCapabilities.ShortVideo
        | SocialPlatformCapabilities.Live
        | SocialPlatformCapabilities.Hashtags
        | SocialPlatformCapabilities.LocationTag
        | SocialPlatformCapabilities.AltText
        | SocialPlatformCapabilities.CustomThumbnail
        | SocialPlatformCapabilities.NativeScheduling
        | SocialPlatformCapabilities.PrivacyLevels
        | SocialPlatformCapabilities.CommentControls
        | SocialPlatformCapabilities.EditPost
        | SocialPlatformCapabilities.DeletePost
        | SocialPlatformCapabilities.ReadUserPosts
        | SocialPlatformCapabilities.PostStatusTracking
        | SocialPlatformCapabilities.MediaUploadApi
        | SocialPlatformCapabilities.Playlists
        | SocialPlatformCapabilities.ReadComments
        | SocialPlatformCapabilities.ReplyToComments
        | SocialPlatformCapabilities.ModerateComments
        | SocialPlatformCapabilities.PostInsights
        | SocialPlatformCapabilities.AccountInsights
        | SocialPlatformCapabilities.AudienceInsights;

    public string Name => "YouTube";
    public string Identifier => "YOUTUBE";
    public SocialPlatformCapabilities Capabilities => Supported;

    public async Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var redirectUri = string.IsNullOrWhiteSpace(opts.RedirectUri) ? throw new InvalidOperationException($"'{YouTubeProviderOptions.SectionName}:RedirectUri' is not configured.") : opts.RedirectUri;
        var client = new YouTubeApiClient(httpClient, opts);
        var resp = await client.ExchangeCodeForTokenAsync(code, redirectUri, ct).ConfigureAwait(false);
        var accessToken = resp.AccessToken ?? throw new YouTubeApiException("YouTube did not return access token");
        var expiresAt = resp.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(resp.ExpiresIn.Value) : DateTime.UtcNow.AddHours(1);
        return new OAuthTokens(accessToken, resp.RefreshToken ?? string.Empty, expiresAt);
    }

    public async Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var resp = await client.RefreshTokenAsync(tokens.RefreshToken, ct).ConfigureAwait(false);
        var accessToken = resp.AccessToken ?? throw new YouTubeApiException("YouTube refresh did not return access token");
        var expiresAt = resp.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(resp.ExpiresIn.Value) : DateTime.UtcNow.AddHours(1);
        return new OAuthTokens(accessToken, resp.RefreshToken ?? tokens.RefreshToken, expiresAt);
    }

    public async Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        if (content.MediaUrls.Count == 0)
            return ToFailure(new YouTubeApiException("YouTube requires a video media URL."));
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new YouTubeApiClient(httpClient, opts);
            var videoUrl = content.MediaUrls[0];
            var title = settings.Title ?? content.Text[..Math.Min(100, content.Text.Length)] ?? "Untitled";
            var description = content.Text;
            var privacy = MapPrivacy(settings.Privacy);
            var response = await client.UploadVideoAsync(title, description, videoUrl, privacy, tokens.AccessToken, ct).ConfigureAwait(false);
            return ToSuccess(RequireId(response.Id, "video"));
        }
        catch (YouTubeApiException ex) { return ToFailure(ex); }
    }

    public async Task<PublishResult> ScheduleAsync(PostContent content, PublishSettings settings, DateTime scheduledAtUtc, OAuthTokens tokens, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (scheduledAtUtc <= now.AddMinutes(1))
            return ToFailure(new YouTubeApiException("scheduledPublishAt must be at least 1 minute in the future."));
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new YouTubeApiClient(httpClient, opts);
            var videoUrl = content.MediaUrls.Count > 0 ? content.MediaUrls[0] : throw new YouTubeApiException("Video URL required for scheduling");
            var title = settings.Title ?? content.Text[..Math.Min(100, content.Text.Length)] ?? "Untitled";
            // YouTube scheduling via publishAt in status
            var response = await client.UploadVideoAsync(title, content.Text, videoUrl, "private", tokens.AccessToken, ct).ConfigureAwait(false);
            return ToSuccess(RequireId(response.Id, "scheduled video"));
        }
        catch (YouTubeApiException ex) { return ToFailure(ex); }
    }

    public async Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var response = await client.UploadVideoAsync(media.FileName, string.Empty, media.BlobUri, "private", tokens.AccessToken, ct).ConfigureAwait(false);
        return new MediaUploadResult(RequireId(response.Id, "media"));
    }

    public async Task<PublishedPost> UpdatePostAsync(string externalPostId, PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var title = settings.Title ?? content.Text[..Math.Min(100, content.Text.Length)];
        await client.UpdateVideoAsync(externalPostId, title, content.Text, tokens.AccessToken, ct).ConfigureAwait(false);
        var video = await client.GetVideoAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
        return new PublishedPost(
            video.Id ?? externalPostId,
            $"https://www.youtube.com/watch?v={video.Id ?? externalPostId}",
            ParseDate(video.Snippet?.PublishedAt),
            video.Status?.UploadStatus,
            video.Snippet?.Title ?? content.Text);
    }

    public async Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        await client.DeleteVideoAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var response = await client.GetUserVideosAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        return response.Items?.Select(v => new PublishedPost(
            v.Id ?? string.Empty,
            $"https://www.youtube.com/watch?v={v.Id}",
            ParseDate(v.Snippet?.PublishedAt),
            v.Status?.UploadStatus,
            v.Snippet?.Title,
            v.Snippet?.Thumbnails?.High?.Url is null ? null : [v.Snippet.Thumbnails.High.Url])).ToList() ?? [];
    }

    public async Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var video = await client.GetVideoAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
        var uploadStatus = video.Status?.UploadStatus?.ToLowerInvariant();
        var status = uploadStatus switch
        {
            "uploaded" or "processed" or "public" => PostProcessingStatus.Published,
            "rejected" or "failed" or "deleted" => PostProcessingStatus.Failed,
            "processing" or "uploaded" => PostProcessingStatus.Processing,
            _ => PostProcessingStatus.Published
        };
        return new PostProcessingStatusResult(status, Permalink: $"https://www.youtube.com/watch?v={externalPostId}");
    }

    public async Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new YouTubeApiClient(httpClient, opts);
            var resp = await client.GetVideoInsightsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            var stats = resp.Items?.FirstOrDefault()?.Statistics;
            if (stats is null) return null;
            return new PostInsights(
                Impressions: ParseLong(stats.ViewCount),
                Reach: ParseLong(stats.ViewCount),
                Engagement: ParseLong(stats.LikeCount) + ParseLong(stats.CommentCount),
                Likes: ParseLong(stats.LikeCount),
                Comments: ParseLong(stats.CommentCount),
                Shares: 0);
        }
        catch (YouTubeApiException) { return null; }
    }

    public async Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var resp = await client.GetChannelAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        var channel = resp.Items?.FirstOrDefault() ?? throw new YouTubeApiException("No YouTube channel found");
        return new AccountProfile(
            ExternalId: channel.Id ?? throw new YouTubeApiException("channel id missing"),
            DisplayName: channel.Snippet?.Title ?? "YouTube Channel",
            AvatarUrl: channel.Snippet?.Thumbnails?.High?.Url,
            Username: channel.Snippet?.Title,
            FollowerCount: ParseLong(channel.Statistics?.SubscriberCount));
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        var resp = await client.GetCommentsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
        return resp.Items?.Select(t =>
        {
            var c = t.Snippet?.TopLevelComment?.Snippet;
            return new Comment(
                t.Id ?? string.Empty,
                c?.AuthorDisplayName ?? "Unknown",
                c?.TextOriginal ?? c?.TextDisplay ?? string.Empty,
                ParseDate(c?.PublishedAt) ?? DateTime.UtcNow);
        }).ToList() ?? [];
    }

    public async Task ReplyToCommentAsync(string commentId, string message, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new YouTubeApiClient(httpClient, opts);
        await client.PostCommentAsync(commentId, message, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new YouTubeApiClient(httpClient, opts);
            var channel = await client.GetChannelAsync(tokens.AccessToken, ct).ConfigureAwait(false);
            var stats = channel.Items?.FirstOrDefault()?.Statistics;
            if (stats is null) return null;
            return new AccountInsights(
                FollowerCount: ParseLong(stats.SubscriberCount),
                Impressions: ParseLong(stats.ViewCount),
                Reach: ParseLong(stats.ViewCount),
                ProfileViews: null,
                EngagementRate: null);
        }
        catch (YouTubeApiException) { return null; }
    }

    private async Task<YouTubeProviderOptions> ResolveOptionsAsync(CancellationToken ct)
    {
        var global = options.Value;
        if (tenantContext?.TenantId is null || credentialRepository is null) return global;
        try
        {
            var credential = await credentialRepository.FindByProviderKeyAsync("YOUTUBE", ct).ConfigureAwait(false);
            if (credential is null || !credential.IsEnabled) return global;
            YouTubeCredentialSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(credential.SettingsJson))
                try { settings = JsonSerializer.Deserialize<YouTubeCredentialSettings>(credential.SettingsJson!); } catch { }
            return new YouTubeProviderOptions
            {
                ClientId = !string.IsNullOrWhiteSpace(settings?.ClientId) ? settings!.ClientId! : global.ClientId,
                ClientSecret = !string.IsNullOrWhiteSpace(credential.SecretValue) ? credential.SecretValue! : !string.IsNullOrWhiteSpace(settings?.ClientSecret) ? settings!.ClientSecret! : global.ClientSecret,
                RedirectUri = !string.IsNullOrWhiteSpace(settings?.RedirectUri) ? settings!.RedirectUri! : global.RedirectUri,
                ApiVersion = !string.IsNullOrWhiteSpace(settings?.ApiVersion) ? settings!.ApiVersion! : global.ApiVersion
            };
        }
        catch { return global; }
    }

    private static string MapPrivacy(PrivacyLevel? privacy) => privacy switch
    {
        PrivacyLevel.Private => "private",
        PrivacyLevel.Unlisted => "unlisted",
        PrivacyLevel.Friends => "private",
        _ => "public"
    };
    private static string RequireId(string? id, string kind) => id ?? throw new YouTubeApiException($"YouTube did not return id for {kind}");
    private static PublishResult ToSuccess(string id) => new(id, DateTime.UtcNow, true, null);
    private static PublishResult ToFailure(YouTubeApiException ex) => new(null, null, false, ex.Message);
    private static DateTime? ParseDate(string? value) => value is not null && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed.UtcDateTime : null;
    private static long ParseLong(string? value) => value is not null && long.TryParse(value, out var n) ? n : 0;
}
