namespace PostForge.Domain.Providers.Contracts;

public enum PostProcessingStatus
{
    Processing,
    Review,
    Published,
    Failed,
    Expired
}

public record PostProcessingStatusResult(
    PostProcessingStatus Status,
    string? ErrorMessage = null,
    string? Permalink = null);