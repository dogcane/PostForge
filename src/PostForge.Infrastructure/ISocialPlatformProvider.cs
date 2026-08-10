using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure;

public interface ISocialPlatformProvider
{
    string Name { get; }
    string Identifier { get; }
    SocialPlatform Platform { get; }
    SocialPlatformCapabilities Capabilities { get; }

    bool Supports(SocialPlatformCapabilities capability) => Capabilities.HasFlag(capability);

    // ---- Core (OAuth + publishing + insights) ----
    Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct);
    Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct);
    Task<PublishResultDto> PublishAsync(PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct);
    Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct);

    // ---- Account ----
    Task<AccountProfileDto> GetAccountProfileAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support account profile retrieval.");

    // ---- Publishing extensions ----
    Task<PublishResultDto> PublishCarouselAsync(PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support carousel publishing.");

    Task<PublishResultDto> PublishStoryAsync(PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support story publishing.");

    Task<PublishResultDto> PublishLiveAsync(PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support live streaming.");

    Task<PublishResultDto> ScheduleAsync(PostContentDto content, PublishSettingsDto settings, DateTime scheduledAtUtc, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support native scheduling.");

    // ---- Media ----
    Task<MediaUploadResultDto> UploadMediaAsync(MediaUploadDto media, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support a dedicated media upload API.");

    // ---- Post management ----
    Task<PublishedPostDto> UpdatePostAsync(string externalPostId, PostContentDto content, PublishSettingsDto settings, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support editing published posts.");

    Task DeletePostAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support deleting published posts.");

    Task<IReadOnlyList<PublishedPostDto>> GetUserPostsAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support listing published posts.");

    Task<PostProcessingStatusDto> GetPostStatusAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support post status tracking.");

    // ---- Engagement ----
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support reading comments.");

    Task ReplyToCommentAsync(string commentId, string message, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support replying to comments.");

    Task ModerateCommentAsync(string commentId, CommentModerationAction action, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support comment moderation.");

    // ---- Insights ----
    Task<AccountInsightsDto?> GetAccountInsightsAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotSupportedException($"'{Identifier}' does not support account insights.");
}
