using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.YouTube.Tests;

public class YouTubeProviderTests
{
    private static SocialPlatformCapabilities Capabilities { get; } =
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

    [Fact]
    public void Provider_ShouldExposeExpectedMetadataAndCapabilities()
    {
        ISocialPlatformProvider provider = new YouTubeProvider();

        provider.Name.Should().Be("YouTube");
        provider.Identifier.Should().Be("YOUTUBE");
        provider.Capabilities.Should().Be(Capabilities);

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"YOUTUBE should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
        }
    }

    [Fact]
    public void PublishCarouselAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new YouTubeProvider();
        var act = () => provider.PublishCarouselAsync(
            new PostContent("text", ["https://cdn.example.com/1.jpg"], "YOUTUBE"),
            new PublishSettings(),
            new OAuthTokens("token", "refresh", DateTime.UtcNow),
            CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void CoreMethods_ShouldRemainPhaseStubs()
    {
        ISocialPlatformProvider provider = new YouTubeProvider();
        var tokens = new OAuthTokens("token", "refresh", DateTime.UtcNow);

        provider.Invoking(p => p.ExchangeAuthorizationCodeAsync("code", CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.RefreshTokenAsync(tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.PublishAsync(new PostContent("text", [], provider.Identifier), new PublishSettings(), tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.GetInsightsAsync("post-1", tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
    }
}