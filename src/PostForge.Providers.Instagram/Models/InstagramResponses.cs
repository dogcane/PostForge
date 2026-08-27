using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostForge.Providers.Instagram.Models;

internal sealed record InstagramError(
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("code")] int? Code,
    [property: JsonPropertyName("error_subcode")] int? ErrorSubcode,
    [property: JsonPropertyName("fbtrace_id")] string? FbTraceId);

internal sealed record InstagramErrorResponse([property: JsonPropertyName("error")] InstagramError? Error);

internal sealed record OAuthTokenResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("expires_in")] long? ExpiresIn);

internal sealed record InstagramIdResponse([property: JsonPropertyName("id")] string? Id);

internal sealed record InstagramUserResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("account_type")] string? AccountType,
    [property: JsonPropertyName("media_count")] long? MediaCount,
    [property: JsonPropertyName("followers_count")] long? FollowersCount,
    [property: JsonPropertyName("profile_picture_url")] string? ProfilePictureUrl);

internal sealed record InstagramMediaResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("caption")] string? Caption,
    [property: JsonPropertyName("media_type")] string? MediaType,
    [property: JsonPropertyName("media_url")] string? MediaUrl,
    [property: JsonPropertyName("permalink")] string? Permalink,
    [property: JsonPropertyName("timestamp")] string? Timestamp,
    [property: JsonPropertyName("status_code")] string? StatusCode);

internal sealed record InstagramMediaCollectionResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<InstagramMediaResponse>? Data,
    [property: JsonPropertyName("paging")] JsonElement? Paging);

internal sealed record InstagramContainerStatusResponse(
    [property: JsonPropertyName("status_code")] string? StatusCode,
    [property: JsonPropertyName("status")] string? Status);

internal sealed record InstagramCommentResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("timestamp")] string? Timestamp,
    [property: JsonPropertyName("username")] string? Username);

internal sealed record InstagramCommentCollectionResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<InstagramCommentResponse>? Data);

internal sealed record InstagramSuccessResponse([property: JsonPropertyName("success")] bool Success);

internal sealed record InstagramInsightsResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<InstagramInsightResult>? Data);

internal sealed record InstagramInsightResult(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("period")] string? Period,
    [property: JsonPropertyName("values")] IReadOnlyList<InstagramInsightValue>? Values,
    [property: JsonPropertyName("total_value")] InstagramInsightTotalValue? TotalValue);

internal sealed record InstagramInsightValue([property: JsonPropertyName("value")] JsonElement Value);

internal sealed record InstagramInsightTotalValue([property: JsonPropertyName("value")] long Value);
