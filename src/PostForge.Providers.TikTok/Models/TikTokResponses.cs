using System.Text.Json.Serialization;

namespace PostForge.Providers.TikTok.Models;

internal sealed record TikTokErrorResponse(
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription,
    [property: JsonPropertyName("log_id")] string? LogId);

internal sealed record TikTokOAuthResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] long? ExpiresIn,
    [property: JsonPropertyName("refresh_expires_in")] long? RefreshExpiresIn,
    [property: JsonPropertyName("open_id")] string? OpenId,
    [property: JsonPropertyName("scope")] string? Scope);

internal sealed record TikTokDataWrapper<T>(
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("error")] TikTokApiError? Error,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("log_id")] string? LogId);

internal sealed record TikTokApiError(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("log_id")] string? LogId);

internal sealed record TikTokInitResponse(
    [property: JsonPropertyName("publish_id")] string? PublishId,
    [property: JsonPropertyName("upload_url")] string? UploadUrl);

internal sealed record TikTokPublishStatusResponse(
    [property: JsonPropertyName("publish_id")] string? PublishId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("publicaly_available_post_id")] string? PublicPostId,
    [property: JsonPropertyName("fail_reason")] string? FailReason,
    [property: JsonPropertyName("uploaded_time")] string? UploadedTime);

internal sealed record TikTokUserInfoResponse(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("open_id")] string? OpenId,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("follower_count")] long? FollowerCount);

internal sealed record TikTokVideoListResponse(
    [property: JsonPropertyName("videos")] IReadOnlyList<TikTokVideoItem>? Videos);

internal sealed record TikTokVideoItem(
    [property: JsonPropertyName("video_id")] string? VideoId,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("cover_image_url")] string? CoverImageUrl,
    [property: JsonPropertyName("create_time")] long? CreateTime,
    [property: JsonPropertyName("share_url")] string? ShareUrl,
    [property: JsonPropertyName("view_count")] long? ViewCount,
    [property: JsonPropertyName("like_count")] long? LikeCount,
    [property: JsonPropertyName("comment_count")] long? CommentCount);

internal sealed record TikTokInsightsResponse(
    [property: JsonPropertyName("view_count")] long? ViewCount,
    [property: JsonPropertyName("like_count")] long? LikeCount,
    [property: JsonPropertyName("comment_count")] long? CommentCount,
    [property: JsonPropertyName("share_count")] long? ShareCount);
