namespace PostForge.Infrastructure.Dtos;

public enum PostProcessingStatus
{
    Processing,
    Review,
    Published,
    Failed,
    Expired
}

public record PostProcessingStatusDto(
    PostProcessingStatus Status,
    string? ErrorMessage = null,
    string? Permalink = null);
