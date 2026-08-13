using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Domain.Providers;

public interface ISocialPlatformProvider
{
    string Name { get; }
    string Identifier { get; }
    SocialPlatformCapabilities Capabilities { get; }

    bool Supports(SocialPlatformCapabilities capability) => Capabilities.HasFlag(capability);

    // ---- Core (OAuth + publishing + insights) ----
    Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct);
    Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct);
    Task<PublishResult> PublishAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct);
    Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct);

    // ---- Account ----
    Task<AccountProfile> GetAccountProfileAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support account profile retrieval.");

    // ---- Publishing extensions ----
    Task<PublishResult> PublishCarouselAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support carousel publishing.");

    Task<PublishResult> PublishStoryAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support story publishing.");

    Task<PublishResult> PublishLiveAsync(PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support live streaming.");

    Task<PublishResult> ScheduleAsync(PostContent content, PublishSettings settings, DateTime scheduledAtUtc, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support native scheduling.");

    // ---- Media ----
    Task<MediaUploadResult> UploadMediaAsync(MediaUpload media, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support a dedicated media upload API.");

    // ---- Post management ----
    Task<PublishedPost> UpdatePostAsync(string externalPostId, PostContent content, PublishSettings settings, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support editing published posts.");

    Task DeletePostAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support deleting published posts.");

    Task<IReadOnlyList<PublishedPost>> GetUserPostsAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support listing published posts.");

    Task<PostProcessingStatusResult> GetPostStatusAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support post status tracking.");

    // ---- Engagement ----
    Task<IReadOnlyList<Comment>> GetCommentsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support reading comments.");

    Task ReplyToCommentAsync(string commentId, string message, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support replying to comments.");

    Task ModerateCommentAsync(string commentId, CommentModerationAction action, OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support comment moderation.");

    // ---- Insights ----
    Task<AccountInsights?> GetAccountInsightsAsync(OAuthTokens tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support account insights.");
}