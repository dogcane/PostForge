using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.Instagram.Tests;

public class InstagramProviderTests
{
    private static SocialPlatformCapabilities Capabilities { get; } =
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

    [Fact]
    public void Provider_ShouldExposeExpectedMetadataAndCapabilities()
    {
        ISocialPlatformProvider provider = new InstagramProvider();

        provider.Name.Should().Be("Instagram");
        provider.Identifier.Should().Be("INSTAGRAM");
        provider.Capabilities.Should().Be(Capabilities);

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"INSTAGRAM should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
        }
    }

    [Fact]
    public void ScheduleAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new InstagramProvider();
        var act = () => provider.ScheduleAsync(
            new PostContent("text", [], "INSTAGRAM"),
            new PublishSettings(),
            DateTime.UtcNow,
            new OAuthTokens("token", "refresh", DateTime.UtcNow),
            CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void CoreMethods_ShouldRemainPhaseStubs()
    {
        ISocialPlatformProvider provider = new InstagramProvider();
        var tokens = new OAuthTokens("token", "refresh", DateTime.UtcNow);

        provider.Invoking(p => p.ExchangeAuthorizationCodeAsync("code", CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.RefreshTokenAsync(tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.PublishAsync(new PostContent("text", [], provider.Identifier), new PublishSettings(), tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.GetInsightsAsync("post-1", tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
    }
}
