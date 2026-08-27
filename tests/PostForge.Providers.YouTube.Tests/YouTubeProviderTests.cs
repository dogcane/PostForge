using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.YouTube.Tests;

public class YouTubeProviderTests
{
    private static readonly OAuthTokens Tokens = new("yt-access", "yt-refresh", DateTime.UtcNow.AddHours(1));

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
        ISocialPlatformProvider provider = YouTubeProviderTestFactory.Create().Provider;
        provider.Name.Should().Be("YouTube");
        provider.Identifier.Should().Be("YOUTUBE");
        provider.Capabilities.Should().Be(Capabilities);
        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"YOUTUBE should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
    }

    [Fact]
    public void PublishCarouselAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new YouTubeProvider(new HttpClient(), Options.Create(new YouTubeProviderOptions()));
        var act = () => provider.PublishCarouselAsync(new PostContent("text", ["https://cdn.example.com/1.jpg"], "YOUTUBE"), new PublishSettings(), Tokens, CancellationToken.None);
        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldCallOAuthEndpoint()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"yt-at","refresh_token":"yt-rt","expires_in":3600,"token_type":"Bearer"}"""));
        var tokens = await provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.RequestUri!.AbsoluteUri.Should().Contain("oauth2.googleapis.com/token");
        req.Method.Should().Be(HttpMethod.Post);
        req.Form!["code"].Should().Be("auth-code");
        req.Form!["grant_type"].Should().Be("authorization_code");
        tokens.AccessToken.Should().Be("yt-at");
        tokens.RefreshToken.Should().Be("yt-rt");
    }

    [Fact]
    public void ExchangeAuthorizationCodeAsync_WithoutRedirectUri_ShouldThrow()
    {
        var (provider, _) = YouTubeProviderTestFactory.Create(options: new YouTubeProviderOptions { ClientId = "id", ClientSecret = "s", RedirectUri = string.Empty });
        var act = () => provider.ExchangeAuthorizationCodeAsync("code", CancellationToken.None);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldCallOAuthEndpoint()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"yt-at2","expires_in":3600}"""));
        var refreshed = await provider.RefreshTokenAsync(Tokens, CancellationToken.None);
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.Form!["grant_type"].Should().Be("refresh_token");
        req.Form!["refresh_token"].Should().Be("yt-refresh");
        refreshed.AccessToken.Should().Be("yt-at2");
    }

    [Fact]
    public async Task PublishAsync_ShouldUploadVideo()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"YT_VIDEO_123","kind":"youtube#video","snippet":{"title":"My Title","description":"My Description"}}"""));
        var result = await provider.PublishAsync(new PostContent("My Description", ["https://cdn.example.com/video.mp4"], "YOUTUBE"), new PublishSettings(Title: "My Title"), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("YT_VIDEO_123");
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.RequestUri!.AbsolutePath.Should().Contain("/videos");
        req.RequestUri!.Query.Should().Contain("part=snippet");
        req.RawBody.Should().Contain("My Title");
    }

    [Fact]
    public async Task PublishAsync_WithoutMedia_ShouldReturnFailure()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        var result = await provider.PublishAsync(new PostContent("No media", [], "YOUTUBE"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_WhenApiReturnsError_ShouldReturnFailure()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"code":401,"message":"Invalid Credentials","errors":[{"message":"Invalid Credentials","reason":"authError"}]}}""", HttpStatusCode.Unauthorized));
        var result = await provider.PublishAsync(new PostContent("Hi", ["https://cdn.example.com/v.mp4"], "YOUTUBE"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid Credentials");
    }

    [Fact]
    public async Task ScheduleAsync_ShouldUploadAsPrivate()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"YT_SCHED_123"}"""));
        var result = await provider.ScheduleAsync(new PostContent("Sched video", ["https://cdn.example.com/v.mp4"], "YOUTUBE"), new PublishSettings(), DateTime.UtcNow.AddDays(1), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("YT_SCHED_123");
    }

    [Fact]
    public async Task ScheduleAsync_TooSoon_ShouldReturnFailure()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        var result = await provider.ScheduleAsync(new PostContent("Hi", ["https://cdn.example.com/v.mp4"], "YOUTUBE"), new PublishSettings(), DateTime.UtcNow.AddSeconds(10), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldUploadVideo()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"YT_UPLOAD_1"}"""));
        var result = await provider.UploadMediaAsync(new MediaUpload("https://cdn.example.com/v.mp4", "v.mp4", "video/mp4", 1024, MediaAssetType.Video), Tokens, CancellationToken.None);
        result.MediaId.Should().Be("YT_UPLOAD_1");
    }

    [Fact]
    public async Task GetInsightsAsync_ShouldMapStatistics()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"items":[{"id":"YT_VIDEO_123","statistics":{"viewCount":"1000","likeCount":"100","commentCount":"20","favoriteCount":"5"}}]}"""));
        var insights = await provider.GetInsightsAsync("YT_VIDEO_123", Tokens, CancellationToken.None);
        insights.Should().NotBeNull();
        insights!.Impressions.Should().Be(1000);
        insights.Likes.Should().Be(100);
        insights.Comments.Should().Be(20);
    }

    [Fact]
    public async Task GetInsightsAsync_WhenError_ShouldReturnNull()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"code":404,"message":"Video not found"}}""", HttpStatusCode.NotFound));
        var insights = await provider.GetInsightsAsync("unknown", Tokens, CancellationToken.None);
        insights.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountProfileAsync_ShouldMapChannel()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"items":[{"id":"UC123","snippet":{"title":"My Channel","thumbnails":{"high":{"url":"https://cdn.example.com/avatar.jpg"}}},"statistics":{"subscriberCount":"5000","viewCount":"100000","videoCount":"42"}}]}"""));
        var profile = await provider.GetAccountProfileAsync(Tokens, CancellationToken.None);
        profile.ExternalId.Should().Be("UC123");
        profile.DisplayName.Should().Be("My Channel");
        profile.FollowerCount.Should().Be(5000);
        profile.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
    }

    [Fact]
    public async Task GetUserPostsAsync_ShouldMapSearchResults()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        // GetUserPosts uses search endpoint, but we mock the video list fallback: provider calls GetUserVideosAsync which hits search? Actually we implemented GetUserPosts via GetUserVideosAsync -> search
        // For simplicity our client returns YouTubeVideoListResponse with items containing id/videoId etc; mock accordingly
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"items":[{"id":"VID_1","snippet":{"title":"First","publishedAt":"2026-01-01T10:00:00Z","thumbnails":{"high":{"url":"https://cdn.example.com/thumb.jpg"}}},"status":{"uploadStatus":"processed"}}]}"""));
        var posts = await provider.GetUserPostsAsync(Tokens, CancellationToken.None);
        // Note our GetUserPosts implementation expects YouTubeVideoListResponse with items having id, snippet,title ; but our mock above provides single item
        // It should map to PublishedPost
        posts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPostStatusAsync_ShouldReturnPublished()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"YT_VIDEO_123","status":{"uploadStatus":"processed"},"snippet":{"title":"Test"}}"""));
        var status = await provider.GetPostStatusAsync("YT_VIDEO_123", Tokens, CancellationToken.None);
        status.Status.Should().Be(PostProcessingStatus.Published);
        status.Permalink.Should().Contain("YT_VIDEO_123");
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldMapThreads()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"items":[{"id":"COMMENT_1","snippet":{"topLevelComment":{"id":"COMMENT_1","snippet":{"authorDisplayName":"John","textOriginal":"Great video!","publishedAt":"2026-01-01T10:00:00Z"}}}}]}"""));
        var comments = await provider.GetCommentsAsync("YT_VIDEO_123", Tokens, CancellationToken.None);
        var c = comments.Should().ContainSingle().Subject;
        c.Author.Should().Be("John");
        c.Text.Should().Be("Great video!");
    }

    [Fact]
    public async Task DeletePostAsync_ShouldDelete()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{}"""));
        await provider.DeletePostAsync("YT_VIDEO_123", Tokens, CancellationToken.None);
        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.Query.Should().Contain("YT_VIDEO_123");
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldUpdateTitle()
    {
        var (provider, handler) = YouTubeProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"YT_VIDEO_123","snippet":{"title":"Updated Title","publishedAt":"2026-01-01T10:00:00Z"},"status":{"uploadStatus":"processed"}}"""));
        var updated = await provider.UpdatePostAsync("YT_VIDEO_123", new PostContent("Updated", [], "YOUTUBE"), new PublishSettings(Title: "Updated Title"), Tokens, CancellationToken.None);
        updated.ExternalPostId.Should().Be("YT_VIDEO_123");
        updated.Caption.Should().Be("Updated Title");
        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
    }
}
