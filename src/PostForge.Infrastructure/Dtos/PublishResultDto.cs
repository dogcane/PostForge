namespace PostForge.Infrastructure.Dtos;

public record PublishResultDto(string? ExternalPostId, DateTime? PublishedAtUtc, bool IsSuccess, string? ErrorMessage);
