using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostForge.Providers.Facebook.Models;

internal sealed record FacebookError(
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("code")] int? Code,
    [property: JsonPropertyName("error_subcode")] int? ErrorSubcode,
    [property: JsonPropertyName("fbtrace_id")] string? FbTraceId);

internal sealed record FacebookErrorResponse([property: JsonPropertyName("error")] FacebookError? Error);

internal sealed record OAuthTokenResponse(
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("expires_in")] long? ExpiresIn);

internal sealed record FacebookIdResponse([property: JsonPropertyName("id")] string? Id);

internal sealed record FacebookSuccessResponse([property: JsonPropertyName("success")] bool Success);

internal sealed record FacebookPicture([property: JsonPropertyName("data")] FacebookPictureData? Data);

internal sealed record FacebookPictureData(
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("height")] int? Height,
    [property: JsonPropertyName("width")] int? Width);

internal sealed record FacebookMeResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("fan_count")] long? FanCount,
    [property: JsonPropertyName("picture")] FacebookPicture? Picture);

internal sealed record FacebookPostResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("full_picture")] string? FullPicture,
    [property: JsonPropertyName("permalink_url")] string? PermalinkUrl,
    [property: JsonPropertyName("created_time")] string? CreatedTime,
    [property: JsonPropertyName("is_published")] bool? IsPublished,
    [property: JsonPropertyName("status_type")] string? StatusType);

internal sealed record FacebookPostCollectionResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<FacebookPostResponse>? Data);

internal sealed record FacebookCommentFrom(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("id")] string? Id);

internal sealed record FacebookCommentResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("created_time")] string? CreatedTime,
    [property: JsonPropertyName("from")] FacebookCommentFrom? From);

internal sealed record FacebookCommentCollectionResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<FacebookCommentResponse>? Data);

internal sealed record FacebookInsightsResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<FacebookInsightResult>? Data);

internal sealed record FacebookInsightResult(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("period")] string? Period,
    [property: JsonPropertyName("values")] IReadOnlyList<FacebookInsightValue>? Values);

internal sealed record FacebookInsightValue([property: JsonPropertyName("value")] JsonElement Value);