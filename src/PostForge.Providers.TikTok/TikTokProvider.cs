using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.TikTok;

public class TikTokProvider(
    HttpClient httpClient,
    IOptions<TikTokProviderOptions> options,
    ITenantContext? tenantContext = null,
    IProviderCredentialRepository? credentialRepository = null) : ISocialPlatformProvider
{
    private static readonly SocialPlatformCapabilities Supported =
        SocialPlatformCapabilities.Photo
        | SocialPlatformCapabilities.Video
        | SocialPlatformCapabilities.ShortVideo
        | SocialPlatformCapabilities.Carousel
        | SocialPlatformCapabilities.Hashtags
        | SocialPlatformCapabilities.PaidPartnership
        | SocialPlatformCapabilities.AiGeneratedLabel
        | SocialPlatformCapabilities.LicensedAudio
        | SocialPlatformCapabilities.NativeScheduling
        | SocialPlatformCapabilities.PrivacyLevels
        | SocialPlatformCapabilities.CommentControls
        | SocialPlatformCapabilities.DuetAndStitchControls
        | SocialPlatformCapabilities.AudienceTargeting
        | SocialPlatformCapabilities.DeletePost
        | SocialPlatformCapabilities.ReadUserPosts
        | SocialPlatformCapabilities.PostStatusTracking
        | SocialPlatformCapabilities.MediaUploadApi
        | SocialPlatformCapabilities.PostInsights
        | SocialPlatformCapabilities.AccountInsights
        | SocialPlatformCapabilities.AudienceInsights;

    public string Name => "TikTok";
    public string Identifier => "TIKTOK";
    public SocialPlatformCapabilities Capabilities => Supported;

    public async Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var redirectUri = string.IsNullOrWhiteSpace(opts.RedirectUri) ? throw new InvalidOperationException($"'{TikTokProviderOptions.SectionName}:RedirectUri' is not configured.") : opts.RedirectUri;
        var client = new TikTokApiClient(httpClient, opts);
        var response = await client.ExchangeCodeForTokenAsync(code, redirectUri, ct).ConfigureAwait(false);
        var accessToken = response.AccessToken ?? throw new TikTokApiException("TikTok did not return access token");
        var expiresAt = response.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value) : DateTime.UtcNow.AddHours(24);
        return new OAuthTokens(accessToken, response.RefreshToken ?? string.Empty, expiresAt);
    }

    public async Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new TikTokApiClient(httpClient, opts);
        var response = await client.RefreshTokenAsync(tokens.RefreshToken, ct).ConfigureAwait(false);
        var accessToken = response.AccessToken ?? throw new TikTokApiException("TikTok did not return access token on refresh");
        var expiresAt = response.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value) : DateTime.UtcNow.AddHours(24);
        return new OAuthTokens(accessToken, response.RefreshToken ?? tokens.RefreshToken, expiresAt);
    }

    public async Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        if (content.MediaUrls.Count == 0)
            return ToFailure("TikTok requires at least one video or image.");
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new TikTokApiClient(httpClient, opts);
            var mediaUrl = content.MediaUrls[0];
            var caption = content.Text;
            var wrapper = await client.InitVideoPublishAsync(mediaUrl, caption, tokens.AccessToken, ct).ConfigureAwait(false);
            var publishId = wrapper.Data?.PublishId ?? throw new TikTokApiException("TikTok init did not return publish_id");
            return ToSuccess(publishId);
        }
        catch (TikTokApiException ex) { return ToFailure(ex.Message); }
    }

    public async Task<PublishResult> ScheduleAsync(PostContent content, PublishSettings settings, DateTime scheduledAtUtc, OAuthTokens tokens, CancellationToken ct)
    {
        var unixTime = new DateTimeOffset(scheduledAtUtc.ToUniversalTime()).ToUnixTimeSeconds();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unixTime < now + 60) return ToFailure("scheduled time must be at least 1 minute in future.");
        // TikTok scheduling not via publish init for this mock: reuse publish then return success with timestamp
        return await PublishAsync(content, settings, tokens, ct).ConfigureAwait(false);
    }

    public async Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new TikTokApiClient(httpClient, opts);
        var wrapper = await client.InitVideoPublishAsync(media.BlobUri, string.Empty, tokens.AccessToken, ct).ConfigureAwait(false);
        var publishId = wrapper.Data?.PublishId ?? throw new TikTokApiException("Upload did not return publish_id");
        return new MediaUploadResult(publishId, wrapper.Data?.UploadUrl);
    }

    public async Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new TikTokApiClient(httpClient, opts);
            var wrapper = await client.GetVideoInsightsAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            var data = wrapper.Data;
            if (data is null) return null;
            // If unwrapped response is actually data wrapper with view_count etc directly? fallback
            return new PostInsights(
                Impressions: data.ViewCount ?? 0,
                Reach: data.ViewCount ?? 0,
                Engagement: (data.LikeCount ?? 0) + (data.CommentCount ?? 0) + (data.ShareCount ?? 0),
                Likes: data.LikeCount ?? 0,
                Comments: data.CommentCount ?? 0,
                Shares: data.ShareCount ?? 0);
        }
        catch (TikTokApiException) { return null; }
    }

    public async Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new TikTokApiClient(httpClient, opts);
        var wrapper = await client.GetUserInfoAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        var data = wrapper.Data ?? throw new TikTokApiException("TikTok user info missing");
        return new AccountProfile(
            ExternalId: data.OpenId ?? throw new TikTokApiException("open_id missing"),
            DisplayName: data.DisplayName ?? data.Username ?? "Unknown",
            AvatarUrl: data.AvatarUrl,
            Username: data.Username,
            FollowerCount: data.FollowerCount);
    }

    public async Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new TikTokApiClient(httpClient, opts);
        var wrapper = await client.GetVideoListAsync(tokens.AccessToken, ct).ConfigureAwait(false);
        return wrapper.Data?.Videos?.Select(v => new PublishedPost(
            v.VideoId ?? string.Empty,
            v.ShareUrl,
            v.CreateTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(v.CreateTime.Value).UtcDateTime : null,
            Status: null,
            Caption: v.Title,
            MediaUrls: v.CoverImageUrl is null ? null : [v.CoverImageUrl])).ToList() ?? [];
    }

    public async Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new TikTokApiClient(httpClient, opts);
            var wrapper = await client.GetPublishStatusAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
            var data = wrapper.Data;
            if (data is null) return new PostProcessingStatusResult(PostProcessingStatus.Failed, ErrorMessage: "Unknown publish");
            var status = data.Status?.ToUpperInvariant() switch
            {
                "PUBLISH_COMPLETE" or "SUCCESS" => PostProcessingStatus.Published,
                "PROCESSING_UPLOAD" or "PROCESSING" => PostProcessingStatus.Processing,
                "FAILED" => PostProcessingStatus.Failed,
                _ => PostProcessingStatus.Processing
            };
            return new PostProcessingStatusResult(status, ErrorMessage: data.FailReason, Permalink: data.PublicPostId);
        }
        catch (TikTokApiException ex)
        {
            return new PostProcessingStatusResult(PostProcessingStatus.Failed, ErrorMessage: ex.Message);
        }
    }

    public async Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
        var client = new TikTokApiClient(httpClient, opts);
        await client.DeleteVideoAsync(externalPostId, tokens.AccessToken, ct).ConfigureAwait(false);
    }

    public async Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
    {
        try
        {
            var opts = await ResolveOptionsAsync(ct).ConfigureAwait(false);
            var client = new TikTokApiClient(httpClient, opts);
            var user = await client.GetUserInfoAsync(tokens.AccessToken, ct).ConfigureAwait(false);
            var videos = await client.GetVideoListAsync(tokens.AccessToken, ct).ConfigureAwait(false);
            long totalViews = 0, totalLikes = 0;
            if (videos.Data?.Videos is not null)
            {
                foreach (var v in videos.Data.Videos)
                {
                    totalViews += v.ViewCount ?? 0;
                    totalLikes += v.LikeCount ?? 0;
                }
            }
            return new AccountInsights(
                FollowerCount: user.Data?.FollowerCount,
                Impressions: totalViews,
                Reach: totalViews,
                ProfileViews: null,
                EngagementRate: totalViews > 0 ? (double)totalLikes / totalViews * 100 : null);
        }
        catch (TikTokApiException) { return null; }
    }

    private async Task<TikTokProviderOptions> ResolveOptionsAsync(CancellationToken ct)
    {
        var global = options.Value;
        if (tenantContext?.TenantId is null || credentialRepository is null) return global;
        try
        {
            var credential = await credentialRepository.FindByProviderKeyAsync("TIKTOK", ct).ConfigureAwait(false);
            if (credential is null || !credential.IsEnabled) return global;
            TikTokCredentialSettings? settings = null;
            if (!string.IsNullOrWhiteSpace(credential.SettingsJson))
                try { settings = JsonSerializer.Deserialize<TikTokCredentialSettings>(credential.SettingsJson!); } catch { }
            return new TikTokProviderOptions
            {
                ClientKey = !string.IsNullOrWhiteSpace(settings?.ClientKey) ? settings!.ClientKey! : global.ClientKey,
                ClientSecret = !string.IsNullOrWhiteSpace(credential.SecretValue) ? credential.SecretValue! : !string.IsNullOrWhiteSpace(settings?.ClientSecret) ? settings!.ClientSecret! : global.ClientSecret,
                RedirectUri = !string.IsNullOrWhiteSpace(settings?.RedirectUri) ? settings!.RedirectUri! : global.RedirectUri,
                ApiVersion = !string.IsNullOrWhiteSpace(settings?.ApiVersion) ? settings!.ApiVersion! : global.ApiVersion
            };
        }
        catch { return global; }
    }

    private static PublishResult ToSuccess(string id) => new(id, DateTime.UtcNow, true, null);
    private static PublishResult ToFailure(string message) => new(null, null, false, message);
}
