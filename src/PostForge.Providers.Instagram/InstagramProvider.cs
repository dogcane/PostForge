using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.Instagram;

public class InstagramProvider : ISocialPlatformProvider
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

    public string Name => "Instagram";
    public string Identifier => "INSTAGRAM";
    public SocialPlatformCapabilities Capabilities => Supported;

    public Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("Instagram OAuth code exchange will be implemented in Phase 1.");

    public Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram token refresh will be implemented in Phase 1.");

    public Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram post publishing will be implemented in Phase 1.");

    public Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram insights will be implemented in Phase 1.");
}