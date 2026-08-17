using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.TikTok.Tests;

public class TikTokProviderTests
{
    private static SocialPlatformCapabilities Capabilities { get; } =
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

    [Fact]
    public void Provider_ShouldExposeExpectedMetadataAndCapabilities()
    {
        ISocialPlatformProvider provider = new TikTokProvider();

        provider.Name.Should().Be("TikTok");
        provider.Identifier.Should().Be("TIKTOK");
        provider.Capabilities.Should().Be(Capabilities);

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"TIKTOK should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
        }
    }

    [Fact]
    public void GetCommentsAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new TikTokProvider();
        var act = () => provider.GetCommentsAsync("video-1", new OAuthTokens("token", "refresh", DateTime.UtcNow), CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void CoreMethods_ShouldRemainPhaseStubs()
    {
        ISocialPlatformProvider provider = new TikTokProvider();
        var tokens = new OAuthTokens("token", "refresh", DateTime.UtcNow);

        provider.Invoking(p => p.ExchangeAuthorizationCodeAsync("code", CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.RefreshTokenAsync(tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.PublishAsync(new PostContent("text", [], provider.Identifier), new PublishSettings(), tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.GetInsightsAsync("post-1", tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
    }
}