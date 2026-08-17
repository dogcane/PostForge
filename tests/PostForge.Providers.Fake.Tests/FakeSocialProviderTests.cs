using FluentAssertions;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;
using PostForge.Providers.Fake;

namespace PostForge.Providers.Fake.Tests;

public class FakeSocialProviderTests
{
    private static readonly OAuthTokens Tokens = new("access", "refresh", DateTime.UtcNow.AddHours(1));

    private static PostContent Content(string text = "hello fake")
        => new(text, ["https://fake.local/media/1.png"], "FAKE");

    [Fact]
    public void Provider_ShouldExposeMetadataAndAllCapabilities()
    {
        ISocialPlatformProvider provider = new FakeSocialProvider();

        provider.Name.Should().Be("Fake");
        provider.Identifier.Should().Be("FAKE");

        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
        {
            provider.Supports(flag).Should().BeTrue($"{flag} should be supported");
        }
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldReturnTokens()
    {
        var provider = new FakeSocialProvider();

        var tokens = await provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);

        tokens.AccessToken.Should().Be("fake-access-token-auth-code");
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
        tokens.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewAccessToken()
    {
        var provider = new FakeSocialProvider();

        var refreshed = await provider.RefreshTokenAsync(Tokens, CancellationToken.None);

        refreshed.AccessToken.Should().NotBe(Tokens.AccessToken);
        refreshed.RefreshToken.Should().Be(Tokens.RefreshToken);
        refreshed.ExpiresAtUtc.Should().BeAfter(Tokens.ExpiresAtUtc);
    }

    [Fact]
    public async Task PublishAsync_ShouldCreatePostAndReturnInsights()
    {
        var provider = new FakeSocialProvider();

        var result = await provider.PublishAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None);
        var insights = await provider.GetInsightsAsync(result.ExternalPostId!, Tokens, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().NotBeNullOrEmpty();
        result.PublishedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        insights.Should().NotBeNull();
        insights!.Impressions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetInsightsAsync_ForUnknownPost_ShouldReturnNull()
    {
        var provider = new FakeSocialProvider();

        var insights = await provider.GetInsightsAsync("unknown-post", Tokens, CancellationToken.None);

        insights.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountProfileAsync_ShouldReturnFakeProfile()
    {
        var provider = new FakeSocialProvider();

        var profile = await provider.GetAccountProfileAsync(Tokens, CancellationToken.None);

        profile.ExternalId.Should().NotBeNullOrEmpty();
        profile.DisplayName.Should().NotBeNullOrEmpty();
        profile.FollowerCount.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("PublishCarouselAsync")]
    [InlineData("PublishStoryAsync")]
    [InlineData("PublishLiveAsync")]
    [InlineData("ScheduleAsync")]
    public async Task PublishingExtensions_ShouldSucceed(string method)
    {
        var provider = new FakeSocialProvider();
        var result = method switch
        {
            "PublishCarouselAsync" => await provider.PublishCarouselAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None),
            "PublishStoryAsync" => await provider.PublishStoryAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None),
            "PublishLiveAsync" => await provider.PublishLiveAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None),
            "ScheduleAsync" => await provider.ScheduleAsync(Content(), new PublishSettings(), DateTime.UtcNow.AddHours(1), Tokens, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldReturnMediaId()
    {
        var provider = new FakeSocialProvider();

        var result = await provider.UploadMediaAsync(
            new MediaUpload("https://fake.local/media/1.png", "photo.png", "image/png", 1024, MediaAssetType.Image),
            Tokens,
            CancellationToken.None);

        result.MediaId.Should().NotBeNullOrEmpty();
        result.UploadUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateAndGetUserPosts_ShouldReflectPublish()
    {
        var provider = new FakeSocialProvider();

        var published = await provider.PublishAsync(Content("original"), new PublishSettings(), Tokens, CancellationToken.None);
        var updated = await provider.UpdatePostAsync(published.ExternalPostId!, Content("updated"), new PublishSettings(), Tokens, CancellationToken.None);

        updated.Caption.Should().Be("updated");

        var userPosts = await provider.GetUserPostsAsync(Tokens, CancellationToken.None);
        userPosts.Should().ContainSingle(p => p.ExternalPostId == published.ExternalPostId);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldRemovePost()
    {
        var provider = new FakeSocialProvider();
        var published = await provider.PublishAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None);

        await provider.DeletePostAsync(published.ExternalPostId!, Tokens, CancellationToken.None);

        var status = await provider.GetPostStatusAsync(published.ExternalPostId!, Tokens, CancellationToken.None);
        status.Status.Should().Be(PostProcessingStatus.Failed);
    }

    [Fact]
    public async Task GetPostStatusAsync_ForPublishedPost_ShouldReturnPublished()
    {
        var provider = new FakeSocialProvider();
        var published = await provider.PublishAsync(Content(), new PublishSettings(), Tokens, CancellationToken.None);

        var status = await provider.GetPostStatusAsync(published.ExternalPostId!, Tokens, CancellationToken.None);

        status.Status.Should().Be(PostProcessingStatus.Published);
        status.Permalink.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Comments_ShouldBeReplyable()
    {
        var provider = new FakeSocialProvider();

        var comments = await provider.GetCommentsAsync("any-post", Tokens, CancellationToken.None);
        comments.Should().BeEmpty();

        await provider.ReplyToCommentAsync("comment-1", "thanks!", Tokens, CancellationToken.None);

        await provider.ModerateCommentAsync("comment-1", CommentModerationAction.Hide, Tokens, CancellationToken.None);
    }

    [Fact]
    public async Task GetAccountInsightsAsync_ShouldReturnInsights()
    {
        var provider = new FakeSocialProvider();

        var insights = await provider.GetAccountInsightsAsync(Tokens, CancellationToken.None);

        insights.Should().NotBeNull();
        insights!.FollowerCount.Should().BeGreaterThan(0);
        insights.EngagementRate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResolveViaRegistry_ShouldReturnFakeProvider()
    {
        var provider = new FakeSocialProvider();
        var registry = new PostForge.Infrastructure.Providers.Social.SocialPlatformProviderRegistry([provider]);

        registry.AvailableProviderKeys.Should().Contain("FAKE");
        registry.Resolve("fake").Should().BeSameAs(provider);
    }
}