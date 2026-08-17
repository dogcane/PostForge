using System.Net;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using PostForge.Domain.Providers.Contracts;
using PostForge.Providers.Facebook;

namespace PostForge.Providers.Facebook.Tests;

public class FacebookProviderTests
{
    private static readonly OAuthTokens Tokens = new("page-access-token", string.Empty, DateTime.UtcNow.AddDays(1));

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldCallOAuthEndpointAndMapTokens()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"access_token":"new-access-token","token_type":"bearer","expires_in":5183944}"""));

        var tokens = await provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/oauth/access_token");

        var query = FacebookProviderTestFactory.ParseQuery(request.RequestUri);
        query["code"].Should().Be("auth-code");
        query["client_id"].Should().Be("app-id");
        query["client_secret"].Should().Be("app-secret");
        query["redirect_uri"].Should().Be("https://localhost/callback");

        tokens.AccessToken.Should().Be("new-access-token");
        tokens.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(5183944), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ExchangeAuthorizationCodeAsync_WithoutRedirectUri_ShouldThrowInvalidOperation()
    {
        var (provider, _) = FacebookProviderTestFactory.Create(options: new FacebookProviderOptions
        {
            AppId = "app-id",
            AppSecret = "app-secret",
            RedirectUri = string.Empty
        });

        var act = () => provider.ExchangeAuthorizationCodeAsync("auth-code", CancellationToken.None);

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldExchangeForLongLivedToken()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"access_token":"long-lived-token","token_type":"bearer","expires_in":5183944}"""));

        var refreshed = await provider.RefreshTokenAsync(Tokens, CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/oauth/access_token");

        var query = FacebookProviderTestFactory.ParseQuery(request.RequestUri);
        query["grant_type"].Should().Be("fb_exchange_token");
        query["fb_exchange_token"].Should().Be("page-access-token");

        refreshed.AccessToken.Should().Be("long-lived-token");
    }

    [Fact]
    public async Task PublishAsync_TextOnly_ShouldPostToPageFeed()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_123"}"""));

        var result = await provider.PublishAsync(
            new PostContent("Hello world", [], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ExternalPostId.Should().Be("987654321_123");

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/feed");

        var form = request.Form!;
        form["message"].Should().Be("Hello world");
        form["published"].Should().Be("true");
        form["access_token"].Should().Be("page-access-token");
    }

    [Fact]
    public async Task PublishAsync_WithPhotos_ShouldUploadUnpublishedThenPublishFeedPost()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_1"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_2"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_123"}"""));

        var result = await provider.PublishAsync(
            new PostContent("Check this out", ["https://cdn.example.com/a.jpg", "https://cdn.example.com/b.png"], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Requests.Should().HaveCount(3);

        var first = handler.Requests[0];
        first.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/photos");
        first.Form!["published"].Should().Be("false");

        var second = handler.Requests[1];
        second.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/photos");

        var third = handler.Requests[2];
        third.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/feed");
        var feedForm = third.Form!;
        feedForm["message"].Should().Be("Check this out");
        feedForm["attached_media"].Should().Contain("PHOTO_1");
        feedForm["attached_media"].Should().Contain("PHOTO_2");
    }

    [Fact]
    public async Task PublishAsync_SingleVideo_ShouldPostToVideosEndpoint()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"VIDEO_1"}"""));

        var result = await provider.PublishAsync(
            new PostContent("A video", ["https://cdn.example.com/clip.mp4"], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/videos");

        var form = request.Form!;
        form["file_url"].Should().Be("https://cdn.example.com/clip.mp4");
        form["description"].Should().Be("A video");
        form["published"].Should().Be("true");
    }

    [Fact]
    public async Task PublishAsync_WhenApiReturnsError_ShouldReturnFailureResult()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"error":{"message":"Invalid OAuth access token.","type":"OAuthException","code":190}}""",
            HttpStatusCode.Unauthorized));

        var result = await provider.PublishAsync(
            new PostContent("Hello", [], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid OAuth access token.");
    }

    [Fact]
    public async Task PublishAsync_WithoutConfiguredPageId_ShouldThrow()
    {
        var (provider, _) = FacebookProviderTestFactory.Create(options: new FacebookProviderOptions
        {
            AppId = "app-id",
            AppSecret = "app-secret",
            RedirectUri = "https://localhost/callback"
        });

        var act = () => provider.PublishAsync(
            new PostContent("Hello", [], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ScheduleAsync_ShouldUseUnpublishedAndUnixTimestamp()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_456"}"""));
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        var result = await provider.ScheduleAsync(
            new PostContent("Scheduled post", [], "FACEBOOK"),
            new PublishSettings(),
            scheduledAt,
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/feed");

        var form = request.Form!;
        form["published"].Should().Be("false");
        form["scheduled_publish_time"].Should().Be(
            new DateTimeOffset(scheduledAt).ToUnixTimeSeconds().ToString());
    }

    [Fact]
    public async Task ScheduleAsync_TooSoon_ShouldReturnFailureWithoutCallingApi()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();

        var result = await provider.ScheduleAsync(
            new PostContent("Scheduled post", [], "FACEBOOK"),
            new PublishSettings(),
            DateTime.UtcNow.AddMinutes(1),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("10 minutes");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ScheduleAsync_WithPhotos_ShouldUseTemporaryUploads()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_1"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_456"}"""));

        var result = await provider.ScheduleAsync(
            new PostContent("Scheduled photos", ["https://cdn.example.com/a.jpg"], "FACEBOOK"),
            new PublishSettings(),
            DateTime.UtcNow.AddDays(1),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var uploadForm = handler.Requests[0].Form!;
        uploadForm["temporary"].Should().Be("true");

        var feedForm = handler.Requests[1].Form!;
        feedForm["published"].Should().Be("false");
        feedForm["scheduled_publish_time"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetInsightsAsync_ShouldMapMetrics()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """
            {"data":[
              {"name":"post_impressions","period":"lifetime","values":[{"value":120}]},
              {"name":"post_impressions_unique","period":"lifetime","values":[{"value":90}]},
              {"name":"post_engaged_users","period":"lifetime","values":[{"value":34}]},
              {"name":"post_reactions_like_total","period":"lifetime","values":[{"value":12}]},
              {"name":"post_comments","period":"lifetime","values":[{"value":6}]},
              {"name":"post_shares","period":"lifetime","values":[{"value":3}]}
            ]}
            """));

        var insights = await provider.GetInsightsAsync("987654321_123", Tokens, CancellationToken.None);

        insights.Should().NotBeNull();
        insights!.Impressions.Should().Be(120);
        insights.Reach.Should().Be(90);
        insights.Engagement.Should().Be(34);
        insights.Likes.Should().Be(12);
        insights.Comments.Should().Be(6);
        insights.Shares.Should().Be(3);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321_123/insights");
        FacebookProviderTestFactory.ParseQuery(request.RequestUri)["period"].Should().Be("lifetime");
    }

    [Fact]
    public async Task GetInsightsAsync_WhenApiReturnsError_ShouldReturnNull()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"error":{"message":"(#100) The post you are trying to access is not available.","code":100}}""",
            HttpStatusCode.Forbidden));

        var insights = await provider.GetInsightsAsync("987654321_123", Tokens, CancellationToken.None);

        insights.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountProfileAsync_ShouldMapMe()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """
            {"id":"987654321","name":"My Page","username":"mypage","fan_count":1234,
             "picture":{"data":{"height":200,"width":200,"url":"https://scontent.example.com/me.jpg"}}}
            """));

        var profile = await provider.GetAccountProfileAsync(Tokens, CancellationToken.None);

        profile.ExternalId.Should().Be("987654321");
        profile.DisplayName.Should().Be("My Page");
        profile.Username.Should().Be("mypage");
        profile.FollowerCount.Should().Be(1234);
        profile.AvatarUrl.Should().Be("https://scontent.example.com/me.jpg");

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/me");
        FacebookProviderTestFactory.ParseQuery(request.RequestUri)["fields"].Should().Contain("fan_count");
    }

    [Fact]
    public async Task GetUserPostsAsync_ShouldMapPagePosts()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """
            {"data":[
              {"id":"987654321_1","message":"First","created_time":"2026-01-01T10:00:00+0000",
               "permalink_url":"https://www.facebook.com/first","full_picture":"https://cdn.example.com/1.jpg",
               "status_type":"mobile_status_update"}
            ]}
            """));

        var posts = await provider.GetUserPostsAsync(Tokens, CancellationToken.None);

        var post = posts.Should().ContainSingle().Subject;
        post.ExternalPostId.Should().Be("987654321_1");
        post.Caption.Should().Be("First");
        post.Permalink.Should().Be("https://www.facebook.com/first");
        post.PublishedAtUtc.Should().Be(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        post.MediaUrls.Should().BeEquivalentTo("https://cdn.example.com/1.jpg");
    }

    [Fact]
    public async Task GetPostStatusAsync_WhenPublished_ShouldReturnPublished()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"id":"987654321_123","is_published":true,"permalink_url":"https://www.facebook.com/p/123"}"""));

        var status = await provider.GetPostStatusAsync("987654321_123", Tokens, CancellationToken.None);

        status.Status.Should().Be(PostProcessingStatus.Published);
        status.Permalink.Should().Be("https://www.facebook.com/p/123");
    }

    [Fact]
    public async Task GetPostStatusAsync_WhenNotPublished_ShouldReturnProcessing()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_123","is_published":false}"""));

        var status = await provider.GetPostStatusAsync("987654321_123", Tokens, CancellationToken.None);

        status.Status.Should().Be(PostProcessingStatus.Processing);
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldMapComments()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """
            {"data":[
              {"id":"C_1","from":{"name":"Jane","id":"u1"},"message":"Nice post!","created_time":"2026-01-01T10:00:00+0000"}
            ]}
            """));

        var comments = await provider.GetCommentsAsync("987654321_123", Tokens, CancellationToken.None);

        var comment = comments.Should().ContainSingle().Subject;
        comment.ExternalId.Should().Be("C_1");
        comment.Author.Should().Be("Jane");
        comment.Text.Should().Be("Nice post!");
    }

    [Fact]
    public async Task ReplyToCommentAsync_ShouldPostReply()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"C_REPLY"}"""));

        await provider.ReplyToCommentAsync("C_1", "Thanks!", Tokens, CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/C_1/comments");
        request.Form!["message"].Should().Be("Thanks!");
    }

    [Fact]
    public async Task ModerateCommentAsync_Ban_ShouldThrowNotSupported()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();

        var act = () => provider.ModerateCommentAsync("C_1", CommentModerationAction.Ban, Tokens, CancellationToken.None);

        await act.Should().ThrowAsync<NotSupportedException>();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ModerateCommentAsync_Hide_ShouldSetIsHidden()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"success":true}"""));

        await provider.ModerateCommentAsync("C_1", CommentModerationAction.Hide, Tokens, CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/C_1");
        request.Form!["is_hidden"].Should().Be("true");
    }

    [Fact]
    public async Task DeletePostAsync_ShouldDeletePost()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"success":true}"""));

        await provider.DeletePostAsync("987654321_123", Tokens, CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Delete);
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321_123");
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldUpdateMessageAndFetchPermalink()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_123"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """{"id":"987654321_123","message":"Updated","created_time":"2026-01-01T10:00:00+0000","permalink_url":"https://www.facebook.com/upd"}"""));

        var updated = await provider.UpdatePostAsync(
            "987654321_123",
            new PostContent("Updated", [], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].Form!["message"].Should().Be("Updated");
        updated.ExternalPostId.Should().Be("987654321_123");
        updated.Permalink.Should().Be("https://www.facebook.com/upd");
        updated.Caption.Should().Be("Updated");
    }

    [Fact]
    public async Task GetAccountInsightsAsync_ShouldComputeEngagementRate()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json(
            """
            {"data":[
              {"name":"page_fans","period":"day","values":[{"value":500}]},
              {"name":"page_profile_views","period":"day","values":[{"value":80}]},
              {"name":"page_impressions","period":"day","values":[{"value":1000}]},
              {"name":"page_impressions_unique","period":"day","values":[{"value":800}]},
              {"name":"page_engaged_users","period":"day","values":[{"value":200}]}
            ]}
            """));

        var insights = await provider.GetAccountInsightsAsync(Tokens, CancellationToken.None);

        insights.Should().NotBeNull();
        insights!.FollowerCount.Should().Be(500);
        insights.Impressions.Should().Be(1000);
        insights.Reach.Should().Be(800);
        insights.ProfileViews.Should().Be(80);
        insights.EngagementRate.Should().BeApproximately(20.0, 0.01);
    }

    [Fact]
    public async Task PublishCarouselAsync_WithMultiplePhotos_ShouldAttachAllPhotos()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_1"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_2"}"""));
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_789"}"""));

        var result = await provider.PublishCarouselAsync(
            new PostContent("Carousel", ["https://cdn.example.com/a.jpg", "https://cdn.example.com/b.jpg"], "FACEBOOK"),
            new PublishSettings(),
            Tokens,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Requests.Should().HaveCount(3);
        handler.Requests[2].Form!["attached_media"].Should().Contain("PHOTO_2");
    }

    [Fact]
    public async Task UploadMediaAsync_Photo_ShouldUploadUnpublishedPhoto()
    {
        var (provider, handler) = FacebookProviderTestFactory.Create();
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"PHOTO_9"}"""));

        var result = await provider.UploadMediaAsync(
            new MediaUpload("https://cdn.example.com/a.jpg", "a.jpg", "image/jpeg", 1234, MediaAssetType.Image),
            Tokens,
            CancellationToken.None);

        result.MediaId.Should().Be("PHOTO_9");
        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/v26.0/987654321/photos");
        request.Form!["published"].Should().Be("false");
    }

    [Fact]
    public async Task EnableAppSecretProof_ShouldAddProofToRequests()
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("app-secret"));
        var expectedProof = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes("page-access-token"))).ToLowerInvariant();

        var (provider, handler) = FacebookProviderTestFactory.Create(options: new FacebookProviderOptions
        {
            AppId = "app-id",
            AppSecret = "app-secret",
            RedirectUri = "https://localhost/callback",
            DefaultPageId = "987654321",
            EnableAppSecretProof = true
        });
        handler.Enqueue(FakeHttpMessageHandler.Json("""{"id":"987654321_123"}"""));

        await provider.PublishAsync(new PostContent("Hello", [], "FACEBOOK"), new PublishSettings(), Tokens, CancellationToken.None);

        var form = handler.Requests.Should().ContainSingle().Subject.Form!;
        form["appsecret_proof"].Should().Be(expectedProof);
    }
}