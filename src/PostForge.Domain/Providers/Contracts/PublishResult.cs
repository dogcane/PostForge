namespace PostForge.Domain.Providers.Contracts;

public record PublishResult(string? ExternalPostId, DateTime? PublishedAtUtc, bool IsSuccess, string? ErrorMessage);