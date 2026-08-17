using System.Collections.Concurrent;
using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Providers.Fake;

public class FakeSocialProvider : ISocialPlatformProvider
{
    private static readonly SocialPlatformCapabilities AllCapabilities =
        (SocialPlatformCapabilities)Enum.GetValues<SocialPlatformCapabilities>()
            .Cast<long>()
            .Aggregate(0L, (acc, value) => acc | value);

    private readonly ConcurrentDictionary<string, PublishedPost> _posts = [];
    private readonly ConcurrentDictionary<string, List<Comment>> _commentsByPost = [];
    private int _nextId;

    public string Name => "Fake";
    public string Identifier => "FAKE";
    public SocialPlatformCapabilities Capabilities => AllCapabilities;

    // ---- Core (OAuth + publishing + insights) ----

    public Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => Task.FromResult(new OAuthTokens(
            $"fake-access-token-{code}",
            "fake-refresh-token",
            DateTime.UtcNow.AddHours(1)));

    public Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult(tokens with
        {
            AccessToken = $"fake-access-token-{Guid.NewGuid():N}",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
        });

    public Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        var externalPostId = CreateExternalPostId();
        var now = DateTime.UtcNow;

        _posts[externalPostId] = new PublishedPost(
            ExternalPostId: externalPostId,
            Permalink: $"https://fake.local/posts/{externalPostId}",
            PublishedAtUtc: now,
            Status: "published",
            Caption: content.Text,
            MediaUrls: content.MediaUrls);

        return Task.FromResult(new PublishResult(externalPostId, now, true, null));
    }

    public Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => _posts.ContainsKey(externalPostId)
            ? Task.FromResult<PostInsights?>(new PostInsights(
                Impressions: 1000,
                Reach: 800,
                Engagement: 120,
                Likes: 90,
                Comments: 10,
                Shares: 20))
            : Task.FromResult<PostInsights?>(null);

    // ---- Account ----

    public Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult(new AccountProfile(
            ExternalId: "fake-account-1",
            DisplayName: "Fake Account",
            AvatarUrl: "https://fake.local/avatar.png",
            Username: "fake.account",
            FollowerCount: 1234));

    // ---- Publishing extensions ----

    public Task<PublishResult> PublishCarouselAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => PublishAsync(content, settings, tokens, ct);

    public Task<PublishResult> PublishStoryAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => PublishAsync(content, settings, tokens, ct);

    public Task<PublishResult> PublishLiveAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => PublishAsync(content, settings, tokens, ct);

    public Task<PublishResult> ScheduleAsync(PostContent content, PublishSettings settings, DateTime scheduledAtUtc, OAuthTokens tokens, CancellationToken ct)
        => PublishAsync(content, settings, tokens, ct);

    // ---- Media ----

    public Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult(new MediaUploadResult(
            MediaId: CreateExternalPostId(),
            UploadUrl: $"https://fake.local/uploads/{media.FileName}",
            ExpiresAtUtc: DateTime.UtcNow.AddHours(1)));

    // ---- Post management ----

    public Task<PublishedPost> UpdatePostAsync(string externalPostId, PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
    {
        var existing = _posts.TryGetValue(externalPostId, out var post)
            ? post with { Caption = content.Text, MediaUrls = content.MediaUrls }
            : new PublishedPost(externalPostId, Caption: content.Text, MediaUrls: content.MediaUrls);

        _posts[externalPostId] = existing;
        return Task.FromResult(existing);
    }

    public Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        _posts.TryRemove(externalPostId, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PublishedPost>>(_posts.Values.ToArray());

    public Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult(_posts.TryGetValue(externalPostId, out var post)
            ? new PostProcessingStatusResult(PostProcessingStatus.Published, Permalink: post.Permalink)
            : new PostProcessingStatusResult(PostProcessingStatus.Failed, ErrorMessage: $"Post '{externalPostId}' not found."));

    // ---- Engagement ----

    public Task<IReadOnlyList<Comment>> GetCommentsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
    {
        var comments = _commentsByPost.GetValueOrDefault(externalPostId) ?? [];
        return Task.FromResult<IReadOnlyList<Comment>>(comments.ToArray());
    }

    public Task ReplyToCommentAsync(string commentId, string message, OAuthTokens tokens, CancellationToken ct)
    {
        _commentsByPost.Values.ToList().ForEach(comments =>
        {
            var target = comments.FirstOrDefault(c => c.ExternalId == commentId);
            if (target is not null)
            {
                comments.Add(new Comment(
                    ExternalId: CreateExternalPostId(),
                    Author: "fake.account",
                    Text: message,
                    CreatedAtUtc: DateTime.UtcNow,
                    ReplyToExternalId: target.ExternalId));
            }
        });

        return Task.CompletedTask;
    }

    public Task ModerateCommentAsync(string commentId, CommentModerationAction action, OAuthTokens tokens, CancellationToken ct)
        => Task.CompletedTask;

    // ---- Insights ----

    public Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
        => Task.FromResult<AccountInsights?>(new AccountInsights(
            FollowerCount: 1234,
            Impressions: 50000,
            Reach: 30000,
            ProfileViews: 1500,
            EngagementRate: 4.2));

    private string CreateExternalPostId()
        => $"fake-post-{Interlocked.Increment(ref _nextId)}";
}