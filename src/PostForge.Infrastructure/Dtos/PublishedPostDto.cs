namespace PostForge.Infrastructure.Dtos;

public record PublishedPostDto(
    string ExternalPostId,
    string? Permalink = null,
    DateTime? PublishedAtUtc = null,
    string? Status = null,
    string? Caption = null,
    IReadOnlyList<string>? MediaUrls = null);
