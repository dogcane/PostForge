using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.Facebook.Tests;

public class FacebookProviderMetadataTests
{
    private static SocialPlatformCapabilities Capabilities { get; } =
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

    [Fact]
    public void Provider_ShouldExposeExpectedMetadataAndCapabilities()
    {
        ISocialPlatformProvider provider = FacebookProviderTestFactory.Create().Provider;

        provider.Name.Should().Be("Facebook");
        provider.Identifier.Should().Be("FACEBOOK");
        provider.Capabilities.Should().Be(Capabilities);

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"FACEBOOK should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
        }
    }
}
