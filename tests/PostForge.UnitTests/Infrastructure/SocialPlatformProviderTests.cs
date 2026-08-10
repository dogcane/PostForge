using FluentAssertions;
using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure;
using PostForge.Infrastructure.Dtos;
using PostForge.Infrastructure.Providers.Social;

namespace PostForge.UnitTests.Infrastructure;

public class SocialPlatformProviderTests
{
    private static SocialPlatformCapabilities FacebookCapabilities { get; } =
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

    private static SocialPlatformCapabilities InstagramCapabilities { get; } =
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

    private static SocialPlatformCapabilities TikTokCapabilities { get; } =
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

    private static SocialPlatformCapabilities YouTubeCapabilities { get; } =
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

    public static TheoryData<ISocialPlatformProvider, string, string, SocialPlatform, SocialPlatformCapabilities> ProviderMetadata =>
        new()
        {
            { new FacebookProvider(), "Facebook", "FACEBOOK", SocialPlatform.Facebook, FacebookCapabilities },
            { new InstagramProvider(), "Instagram", "INSTAGRAM", SocialPlatform.Instagram, InstagramCapabilities },
            { new TikTokProvider(), "TikTok", "TIKTOK", SocialPlatform.TikTok, TikTokCapabilities },
            { new YouTubeProvider(), "YouTube", "YOUTUBE", SocialPlatform.YouTube, YouTubeCapabilities },
        };

    public static TheoryData<ISocialPlatformProvider> Providers =>
        new()
        {
            new FacebookProvider(),
            new InstagramProvider(),
            new TikTokProvider(),
            new YouTubeProvider(),
        };

    [Theory]
    [MemberData(nameof(ProviderMetadata))]
    public void Provider_ShouldExposeExpectedMetadataAndCapabilities(
        ISocialPlatformProvider provider, string name, string identifier, SocialPlatform platform, SocialPlatformCapabilities capabilities)
    {
        provider.Name.Should().Be(name);
        provider.Identifier.Should().Be(identifier);
        provider.Platform.Should().Be(platform);
        provider.Capabilities.Should().Be(capabilities);

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().Be(capabilities.HasFlag(flag), $"{identifier} should {(capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
        }
    }

    [Fact]
    public void InstagramProvider_ScheduleAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new InstagramProvider();
        var act = () => provider.ScheduleAsync(
            new PostContentDto("text", [], SocialPlatform.Instagram),
            new PublishSettingsDto(),
            DateTime.UtcNow,
            new OAuthTokensDto("token", "refresh", DateTime.UtcNow),
            CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void TikTokProvider_GetCommentsAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new TikTokProvider();
        var act = () => provider.GetCommentsAsync("video-1", new OAuthTokensDto("token", "refresh", DateTime.UtcNow), CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void YouTubeProvider_PublishCarouselAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new YouTubeProvider();
        var act = () => provider.PublishCarouselAsync(
            new PostContentDto("text", ["https://cdn.example.com/1.jpg"], SocialPlatform.YouTube),
            new PublishSettingsDto(),
            new OAuthTokensDto("token", "refresh", DateTime.UtcNow),
            CancellationToken.None);

        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void CoreMethods_ShouldRemainPhaseStubs(ISocialPlatformProvider provider)
    {
        var tokens = new OAuthTokensDto("token", "refresh", DateTime.UtcNow);

        provider.Invoking(p => p.ExchangeAuthorizationCodeAsync("code", CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.RefreshTokenAsync(tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.PublishAsync(new PostContentDto("text", [], provider.Platform), new PublishSettingsDto(), tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
        provider.Invoking(p => p.GetInsightsAsync("post-1", tokens, CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
    }

    private sealed class TestSocialProvider : ISocialPlatformProvider
    {
        public TestSocialProvider(SocialPlatformCapabilities capabilities) => Capabilities = capabilities;

        public string Name => "Test";
        public string Identifier => "TEST";
        public SocialPlatform Platform => SocialPlatform.Facebook;
        public SocialPlatformCapabilities Capabilities { get; }

        public Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct) => throw new NotImplementedException();
        public Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct) => throw new NotImplementedException();
        public Task<PublishResultDto> PublishAsync(PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct) => throw new NotImplementedException();
        public Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct) => throw new NotImplementedException();
    }

    [Fact]
    public void Supports_DefaultImplementation_ShouldReflectCapabilities()
    {
        ISocialPlatformProvider provider = new TestSocialProvider(SocialPlatformCapabilities.Photo | SocialPlatformCapabilities.Collaborators);

        provider.Supports(SocialPlatformCapabilities.Photo).Should().BeTrue();
        provider.Supports(SocialPlatformCapabilities.Collaborators).Should().BeTrue();
        provider.Supports(SocialPlatformCapabilities.Video).Should().BeFalse();
    }
}
