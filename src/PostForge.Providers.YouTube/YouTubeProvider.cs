using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.YouTube;

public class YouTubeProvider : ISocialPlatformProvider
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

    public Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("YouTube OAuth code exchange will be implemented in Phase 4.");

    public Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube token refresh will be implemented in Phase 4.");

    public Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube video upload will be implemented in Phase 4.");

    public Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube insights will be implemented in Phase 4.");
}