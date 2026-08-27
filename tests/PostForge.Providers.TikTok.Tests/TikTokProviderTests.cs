using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.TikTok.Tests;

public class TikTokProviderTests
{
    private static readonly OAuthTokens Tokens = new("tiktok-access", "tiktok-refresh", DateTime.UtcNow.AddHours(1));

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
        ISocialPlatformProvider provider = TikTokProviderTestFactory.Create().Provider;
        provider.Name.Should().Be("TikTok");
        provider.Identifier.Should().Be("TIKTOK");
        provider.Capabilities.Should().Be(Capabilities);
        foreach (var flag in Enum.GetValues<SocialPlatformCapabilities>().Where(f => f != SocialPlatformCapabilities.None))
            provider.Supports(flag).Should().Be(Capabilities.HasFlag(flag), $"TIKTOK should {(Capabilities.HasFlag(flag) ? "support" : "not support")} {flag}");
    }

    [Fact]
    public void GetCommentsAsync_ShouldThrowNotSupported()
    {
        ISocialPlatformProvider provider = new TikTokProvider(new HttpClient(), Options.Create(new TikTokProviderOptions()));
        var act = () => provider.GetCommentsAsync("video-1", Tokens, CancellationToken.None);
        act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldCallTokenEndpoint()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"tiktok-at","refresh_token":"tiktok-rt","expires_in":86400,"open_id":"open123"}"""));
        var tokens = await provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.Method.Should().Be(HttpMethod.Post);
        req.RequestUri!.AbsolutePath.Should().Be("/v2/oauth/token/");
        req.Form!["client_key"].Should().Be("client-key");
        req.Form!["code"].Should().Be("auth-code");
        req.Form!["grant_type"].Should().Be("authorization_code");
        tokens.AccessToken.Should().Be("tiktok-at");
        tokens.RefreshToken.Should().Be("tiktok-rt");
    }

    [Fact]
    public void ExchangeAuthorizationCodeAsync_WithoutRedirectUri_ShouldThrow()
    {
        var (provider, _) = TikTokProviderTestFactory.Create(options: new TikTokProviderOptions { ClientKey = "k", ClientSecret = "s", RedirectUri = string.Empty });
        var act = () => provider.ExchangeAuthorizationCodeAsync("code", CancellationToken.None);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldCallTokenEndpointWithRefreshGrant()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"access_token":"new-at","refresh_token":"new-rt","expires_in":86400}"""));
        var refreshed = await provider.RefreshTokenAsync(Tokens, CancellationToken.None);
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.Form!["grant_type"].Should().Be("refresh_token");
        req.Form!["refresh_token"].Should().Be("tiktok-refresh");
        refreshed.AccessToken.Should().Be("new-at");
    }

    [Fact]
    public async Task PublishAsync_ShouldInitVideoPublish()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"publish_id":"PUBLISH_123","upload_url":"https://upload.tiktok.com/123"}}"""));
        var result = await provider.PublishAsync(new PostContent("Hello TikTok", ["https://cdn.example.com/v.mp4"], "TIKTOK"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("PUBLISH_123");
        var req = handler.Requests.Should().ContainSingle().Subject;
        req.RequestUri!.AbsolutePath.Should().Be("/v2/post/publish/inbox/video/init/");
        req.RawBody.Should().Contain("https://cdn.example.com/v.mp4");
        req.RawBody.Should().Contain("Hello TikTok");
    }

    [Fact]
    public async Task PublishAsync_WithoutMedia_ShouldReturnFailure()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        var result = await provider.PublishAsync(new PostContent("No media", [], "TIKTOK"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_WhenApiReturnsError_ShouldReturnFailure()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"code":"access_token_invalid","message":"Invalid token"}}""", HttpStatusCode.Unauthorized));
        var result = await provider.PublishAsync(new PostContent("Hi", ["https://cdn.example.com/v.mp4"], "TIKTOK"), new PublishSettings(), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ScheduleAsync_ShouldSucceed()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"publish_id":"SCHED_123"}}"""));
        var result = await provider.ScheduleAsync(new PostContent("Sched", ["https://cdn.example.com/v.mp4"], "TIKTOK"), new PublishSettings(), DateTime.UtcNow.AddDays(1), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ScheduleAsync_TooSoon_ShouldReturnFailure()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        var result = await provider.ScheduleAsync(new PostContent("Hi", ["https://cdn.example.com/v.mp4"], "TIKTOK"), new PublishSettings(), DateTime.UtcNow.AddSeconds(10), Tokens, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadMediaAsync_ShouldInitUpload()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"publish_id":"UPLOAD_1","upload_url":"https://upload.tiktok.com/u1"}}"""));
        var result = await provider.UploadMediaAsync(new MediaUpload("https://cdn.example.com/v.mp4", "v.mp4", "video/mp4", 1024, MediaAssetType.Video), Tokens, CancellationToken.None);
        result.MediaId.Should().Be("UPLOAD_1");
        result.UploadUrl.Should().Be("https://upload.tiktok.com/u1");
    }

    [Fact]
    public async Task GetInsightsAsync_ShouldMapCounts()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        // TikTok insights returns view_count etc inside data wrapper; we mock the wrapper
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"view_count":5000,"like_count":300,"comment_count":40,"share_count":10}}"""));
        var insights = await provider.GetInsightsAsync("PUBLISH_123", Tokens, CancellationToken.None);
        insights.Should().NotBeNull();
        insights!.Impressions.Should().Be(5000);
        insights.Likes.Should().Be(300);
        insights.Comments.Should().Be(40);
        insights.Shares.Should().Be(10);
    }

    [Fact]
    public async Task GetInsightsAsync_WhenError_ShouldReturnNull()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"error":{"code":"not_found","message":"Video not found"}}""", HttpStatusCode.NotFound));
        var insights = await provider.GetInsightsAsync("unknown", Tokens, CancellationToken.None);
        insights.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountProfileAsync_ShouldMapUserInfo()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"open_id":"open123","display_name":"TikTok User","username":"tiktok.user","avatar_url":"https://cdn.example.com/avatar.jpg","follower_count":9876}}"""));
        var profile = await provider.GetAccountProfileAsync(Tokens, CancellationToken.None);
        profile.ExternalId.Should().Be("open123");
        profile.DisplayName.Should().Be("TikTok User");
        profile.Username.Should().Be("tiktok.user");
        profile.FollowerCount.Should().Be(9876);
    }

    [Fact]
    public async Task GetUserPostsAsync_ShouldMapVideoList()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"videos":[{"video_id":"VID_1","title":"First video","cover_image_url":"https://cdn.example.com/cover.jpg","create_time":1700000000,"share_url":"https://www.tiktok.com/@user/video/1","view_count":123}]} }"""));
        var posts = await provider.GetUserPostsAsync(Tokens, CancellationToken.None);
        var p = posts.Should().ContainSingle().Subject;
        p.ExternalPostId.Should().Be("VID_1");
        p.Caption.Should().Be("First video");
        p.Permalink.Should().Be("https://www.tiktok.com/@user/video/1");
    }

    [Fact]
    public async Task GetPostStatusAsync_WhenPublished_ShouldReturnPublished()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"publish_id":"PUBLISH_123","status":"PUBLISH_COMPLETE","publicaly_available_post_id":"VID_123"}}"""));
        var status = await provider.GetPostStatusAsync("PUBLISH_123", Tokens, CancellationToken.None);
        status.Status.Should().Be(PostProcessingStatus.Published);
        status.Permalink.Should().Be("VID_123");
    }

    [Fact]
    public async Task GetPostStatusAsync_WhenProcessing_ShouldReturnProcessing()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"publish_id":"PUBLISH_123","status":"PROCESSING_UPLOAD"}}"""));
        var status = await provider.GetPostStatusAsync("PUBLISH_123", Tokens, CancellationToken.None);
        status.Status.Should().Be(PostProcessingStatus.Processing);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldCallDelete()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{}}"""));
        await provider.DeletePostAsync("VID_123", Tokens, CancellationToken.None);
        handler.Requests[0].RequestUri!.AbsolutePath.Should().Be("/v2/video/delete/");
        handler.Requests[0].RequestUri!.Query.Should().Contain("video_id=VID_123");
    }

    [Fact]
    public async Task GetAccountInsightsAsync_ShouldAggregate()
    {
        var (provider, handler) = TikTokProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"open_id":"open123","follower_count":1000}}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"data":{"videos":[{"view_count":100,"like_count":10},{"view_count":200,"like_count":20}]}}"""));
        var insights = await provider.GetAccountInsightsAsync(Tokens, CancellationToken.None);
        insights.Should().NotBeNull();
        insights!.FollowerCount.Should().Be(1000);
        insights.Impressions.Should().Be(300);
    }
}
