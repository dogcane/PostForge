using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.TikTok;

public class TikTokProvider : ISocialPlatformProvider
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

    public Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("TikTok OAuth code exchange will be implemented in Phase 4.");

    public Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok token refresh will be implemented in Phase 4.");

    public Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok post publishing will be implemented in Phase 4.");

    public Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok insights will be implemented in Phase 4.");
}