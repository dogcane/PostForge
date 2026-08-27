using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.Instagram.Tests;

public class InstagramProviderTests
{
    private static readonly OAuthTokens Tokens = new("ig-access-token", string.Empty, DateTime.UtcNow.AddDays(1));

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
        ISocialPlatformProvider provider = InstagramProviderTestFactory.Create().Provider;
        provider.Name.Should().Be("Instagram");
        provider.Identifier.Should().Be("INSTAGRAM");
        provider.Capabilities.Should().Be(Capabilities);
        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"INSTAGRAM should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
    }

    [Fact]
    public void ScheduleAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new InstagramProvider(
            new HttpClient(), Options.Create(new InstagramProviderOptions()));
        var act = () => provider.ScheduleAsync(new PostContent("text", [], "INSTAGRAM"), new PublishSettings(), DateTime.UtcNow, Tokens, CancellationToken.None);
        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldCallOAuthEndpointAndMapTokens()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"ig-long-token","token_type":"bearer","expires_in":5184000}"""));
        var tokens = await provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);
        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.AbsolutePath.Should().Be("/v22.0/oauth/access_token");
        var query = InstagramProviderTestFactory.ParseQuery(request.RequestUri);
        query["code"].Should().Be("auth-code");
        query["client_id"].Should().Be("app-id");
        query["redirect_uri"].Should().Be("https://localhost/callback");
        tokens.AccessToken.Should().Be("ig-long-token");
        tokens.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(5184000), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ExchangeAuthorizationCodeAsync_WithoutRedirectUri_ShouldThrowInvalidOperation()
    {
        var (provider, _) = InstagramProviderTestFactory.Create(options: new InstagramProviderOptions { AppId = "app-id", AppSecret = "app-secret", RedirectUri = string.Empty, DefaultInstagramUserId = "17841412345678901" });
        var act = () => provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldExchangeForLongLivedToken()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"ig-refreshed","token_type":"bearer","expires_in":5184000}"""));
        var refreshed = await provider.RefreshTokenAsync(Tokens, CancellationToken.None);
        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v22.0/oauth/access_token");
        var query = InstagramProviderTestFactory.ParseQuery(request.RequestUri);
        query["grant_type"].Should().Be("fb_exchange_token");
        query["fb_exchange_token"].Should().Be("ig-access-token");
        refreshed.AccessToken.Should().Be("ig-refreshed");
    }

    [Fact]
    public async Task PublishAsync_SingleImage_ShouldCreateContainerThenPublish()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"1789_CONTAINER"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"1789_MEDIA"}"""));

        var result = await provider.PublishAsync(new PostContent("Hello IG", ["https://cdn.example.com/a.jpg"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("1789_MEDIA");
        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/17841412345678901/media");
        handler.Requests[0].Form!["image_url"].Should().Be("https://cdn.example.com/a.jpg");
        handler.Requests[0].Form!["caption"].Should().Be("Hello IG");
        handler.Requests[1].RequestUri!.AbsolutePath.Should().Be("/v22.0/17841412345678901/media_publish");
        handler.Requests[1].Form!["creation_id"].Should().Be("1789_CONTAINER");
    }

    [Fact]
    public async Task PublishAsync_SingleVideo_ShouldUseVideoUrl()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"CONTAINER_VIDEO"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"MEDIA_VIDEO"}"""));

        var result = await provider.PublishAsync(new PostContent("Reel caption", ["https://cdn.example.com/reel.mp4"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var first = handler.Requests[0];
        first.Form!["video_url"].Should().Be("https://cdn.example.com/reel.mp4");
        first.Form!["media_type"].Should().Be("REELS");
    }

    [Fact]
    public async Task PublishAsync_WithoutMedia_ShouldReturnFailure()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        var result = await provider.PublishAsync(new PostContent("No media", [], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least one");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_WhenApiReturnsError_ShouldReturnFailure()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"message":"Invalid OAuth access token.","type":"OAuthException","code":190}}""", HttpStatusCode.Unauthorized));
        var result = await provider.PublishAsync(new PostContent("Hello", ["https://cdn.example.com/a.jpg"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid OAuth");
    }

    [Fact]
    public async Task PublishAsync_WithoutConfiguredUserId_ShouldThrow()
    {
        var (provider, _) = InstagramProviderTestFactory.Create(options: new InstagramProviderOptions { AppId = "app-id", AppSecret = "app-secret", RedirectUri = "https://localhost/callback", DefaultInstagramUserId = string.Empty });
        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.PublishAsync(new PostContent("Hello", ["https://cdn.example.com/a.jpg"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None));
    }

    [Fact]
    public async Task PublishCarouselAsync_ShouldCreateChildrenAndPublishCarousel()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"CHILD_1"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"CHILD_2"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"CAROUSEL_CONTAINER"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"CAROUSEL_MEDIA"}"""));

        var result = await provider.PublishCarouselAsync(new PostContent("Carousel", ["https://cdn.example.com/a.jpg", "https://cdn.example.com/b.jpg"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("CAROUSEL_MEDIA");
        handler.Requests.Should().HaveCount(4);
        handler.Requests[0].Form!["is_carousel_item"].Should().Be("true");
        handler.Requests[2].Form!["media_type"].Should().Be("CAROUSEL");
        handler.Requests[2].Form!["children"].Should().Contain("CHILD_1");
        handler.Requests[3].Form!["creation_id"].Should().Be("CAROUSEL_CONTAINER");
    }

    [Fact]
    public async Task PublishStoryAsync_ShouldRequireSingleMedia()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        var result = await provider.PublishStoryAsync(new PostContent("Story", [], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishStoryAsync_WithSingleImage_ShouldCreateStoryContainer()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"STORY_CONTAINER"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"STORY_MEDIA"}"""));
        var result = await provider.PublishStoryAsync(new PostContent("Story caption", ["https://cdn.example.com/story.jpg"], "INSTAGRAM"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        handler.Requests[0].Form!["media_type"].Should().Be("STORIES");
    }

    [Fact]
    public async Task GetInsightsAsync_ShouldMapMetrics()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":[{"name":"impressions","period":"lifetime","values":[{"value":1000}]},{"name":"reach","period":"lifetime","values":[{"value":800}]},{"name":"engagement","period":"lifetime","values":[{"value":120}]},{"name":"like_count","period":"lifetime","values":[{"value":80}]},{"name":"comments_count","period":"lifetime","values":[{"value":20}]},{"name":"shares","period":"lifetime","values":[{"value":5}]}]}"""));
        var insights = await provider.GetInsightsAsync("1789_MEDIA", Tokens, CancellationToken.None);
        insights.Should().NotBeNull();
        insights!.Impressions.Should().Be(1000);
        insights.Reach.Should().Be(800);
        insights.Engagement.Should().Be(120);
        insights.Likes.Should().Be(80);
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/1789_MEDIA/insights");
    }

    [Fact]
    public async Task GetInsightsAsync_WhenApiReturnsError_ShouldReturnNull()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"message":"Media not found","code":100}}""", HttpStatusCode.NotFound));
        var insights = await provider.GetInsightsAsync("unknown", Tokens, CancellationToken.None);
        insights.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountProfileAsync_ShouldMapMe()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"17841412345678901","username":"my.ig","account_type":"BUSINESS","followers_count":4321,"profile_picture_url":"https://cdn.example.com/avatar.jpg"}"""));
        var profile = await provider.GetAccountProfileAsync(Tokens, CancellationToken.None);
        profile.ExternalId.Should().Be("17841412345678901");
        profile.Username.Should().Be("my.ig");
        profile.FollowerCount.Should().Be(4321);
        profile.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/me");
    }

    [Fact]
    public async Task GetUserPostsAsync_ShouldMapMedia()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":[{"id":"1789_1","caption":"First","media_type":"IMAGE","media_url":"https://cdn.example.com/1.jpg","permalink":"https://www.instagram.com/p/1","timestamp":"2026-01-01T10:00:00+0000"}]}"""));
        var posts = await provider.GetUserPostsAsync(Tokens, CancellationToken.None);
        var post = posts.Should().ContainSingle().Subject;
        post.ExternalPostId.Should().Be("1789_1");
        post.Caption.Should().Be("First");
        post.Permalink.Should().Be("https://www.instagram.com/p/1");
        post.MediaUrls.Should().BeEquivalentTo("https://cdn.example.com/1.jpg");
    }

    [Fact]
    public async Task GetPostStatusAsync_ShouldReturnPublishedWhenFinished()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"status_code":"FINISHED","status":"Published"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"1789_MEDIA","permalink":"https://www.instagram.com/p/xyz"}"""));
        var status = await provider.GetPostStatusAsync("1789_MEDIA", Tokens, CancellationToken.None);
        status.Status.Should().Be(PostProcessingStatus.Published);
        status.Permalink.Should().Be("https://www.instagram.com/p/xyz");
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldMapComments()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":[{"id":"C_1","text":"Nice!","timestamp":"2026-01-01T10:00:00+0000","username":"jane"}]}"""));
        var comments = await provider.GetCommentsAsync("1789_MEDIA", Tokens, CancellationToken.None);
        var c = comments.Should().ContainSingle().Subject;
        c.ExternalId.Should().Be("C_1");
        c.Text.Should().Be("Nice!");
        c.Author.Should().Be("jane");
    }

    [Fact]
    public async Task ReplyToCommentAsync_ShouldPostReply()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"REPLY_1"}"""));
        await provider.ReplyToCommentAsync("C_1", "Thanks!", Tokens, CancellationToken.None);
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/C_1/replies");
        handler.Requests[0].Form!["message"].Should().Be("Thanks!");
    }

    [Fact]
    public async Task DeletePostAsync_ShouldDelete()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"success":true}"""));
        await provider.DeletePostAsync("1789_MEDIA", Tokens, CancellationToken.None);
        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/1789_MEDIA");
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldCreateContainer()
    {
        var (provider, handler) = InstagramProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"UPLOAD_CONTAINER"}"""));
        var result = await provider.UploadMediaAsync(new MediaUpload("https://cdn.example.com/a.jpg", "a.jpg", "image/jpeg", 1234, MediaAssetType.Image), Tokens, CancellationToken.None);
        result.MediaId.Should().Be("UPLOAD_CONTAINER");
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v22.0/17841412345678901/media");
    }
}
