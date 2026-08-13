namespace PostForge.Domain.Providers.Contracts;

public record PublishedPost(
    string ExternalPostId,
    string? Permalink = null,
    DateTime? PublishedAtUtc = null,
    string? Status = null,
    string? Caption = null,
    IReadOnlyList<string>? MediaUrls = null);