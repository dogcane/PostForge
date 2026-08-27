using System.Text.Json.Serialization;

namespace PostForge.Providers.YouTube.Models;

internal sealed record YouTubeOAuthResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] long? ExpiresIn,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("scope")] string? Scope);

internal sealed record YouTubeErrorDetail(
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("domain")] string? Domain);

internal sealed record YouTubeError(
    [property: JsonPropertyName("code")] int? Code,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("errors")] IReadOnlyList<YouTubeErrorDetail>? Errors);

internal sealed record YouTubeErrorResponse([property: JsonPropertyName("error")] YouTubeError? Error);

internal sealed record YouTubeVideoUploadResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("kind")] string? Kind,
    [property: JsonPropertyName("snippet")] YouTubeSnippet? Snippet,
    [property: JsonPropertyName("status")] YouTubeStatus? Status);

internal sealed record YouTubeSnippet(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("publishedAt")] string? PublishedAt,
    [property: JsonPropertyName("channelId")] string? ChannelId,
    [property: JsonPropertyName("channelTitle")] string? ChannelTitle,
    [property: JsonPropertyName("thumbnails")] YouTubeThumbnails? Thumbnails);

internal sealed record YouTubeThumbnails(
    [property: JsonPropertyName("default")] YouTubeThumbnail? Default,
    [property: JsonPropertyName("medium")] YouTubeThumbnail? Medium,
    [property: JsonPropertyName("high")] YouTubeThumbnail? High);

internal sealed record YouTubeThumbnail([property: JsonPropertyName("url")] string? Url);

internal sealed record YouTubeStatus(
    [property: JsonPropertyName("privacyStatus")] string? PrivacyStatus,
    [property: JsonPropertyName("uploadStatus")] string? UploadStatus,
    [property: JsonPropertyName("publishAt")] string? PublishAt);

internal sealed record YouTubeVideoListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<YouTubeVideoUploadResponse>? Items,
    [property: JsonPropertyName("pageInfo")] YouTubePageInfo? PageInfo);

internal sealed record YouTubePageInfo(
    [property: JsonPropertyName("totalResults")] int? TotalResults,
    [property: JsonPropertyName("resultsPerPage")] int? ResultsPerPage);

internal sealed record YouTubeChannelResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<YouTubeChannelItem>? Items);

internal sealed record YouTubeChannelItem(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("snippet")] YouTubeChannelSnippet? Snippet,
    [property: JsonPropertyName("statistics")] YouTubeChannelStatistics? Statistics);

internal sealed record YouTubeChannelSnippet(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("thumbnails")] YouTubeThumbnails? Thumbnails);

internal sealed record YouTubeChannelStatistics(
    [property: JsonPropertyName("subscriberCount")] string? SubscriberCount,
    [property: JsonPropertyName("viewCount")] string? ViewCount,
    [property: JsonPropertyName("videoCount")] string? VideoCount);

internal sealed record YouTubeCommentThreadListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<YouTubeCommentThread>? Items);

internal sealed record YouTubeCommentThread(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("snippet")] YouTubeCommentThreadSnippet? Snippet);

internal sealed record YouTubeCommentThreadSnippet(
    [property: JsonPropertyName("topLevelComment")] YouTubeComment? TopLevelComment);

internal sealed record YouTubeComment(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("snippet")] YouTubeCommentSnippet? Snippet);

internal sealed record YouTubeCommentSnippet(
    [property: JsonPropertyName("authorDisplayName")] string? AuthorDisplayName,
    [property: JsonPropertyName("textDisplay")] string? TextDisplay,
    [property: JsonPropertyName("textOriginal")] string? TextOriginal,
    [property: JsonPropertyName("publishedAt")] string? PublishedAt);

internal sealed record YouTubeVideoInsightsResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<YouTubeVideoStatisticsItem>? Items);

internal sealed record YouTubeVideoStatisticsItem(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("statistics")] YouTubeVideoStatistics? Statistics);

internal sealed record YouTubeVideoStatistics(
    [property: JsonPropertyName("viewCount")] string? ViewCount,
    [property: JsonPropertyName("likeCount")] string? LikeCount,
    [property: JsonPropertyName("commentCount")] string? CommentCount,
    [property: JsonPropertyName("favoriteCount")] string? FavoriteCount);
