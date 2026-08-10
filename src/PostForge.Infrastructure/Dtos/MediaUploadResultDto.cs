namespace PostForge.Infrastructure.Dtos;

public record MediaUploadResultDto(
    string MediaId,
    string? UploadUrl = null,
    DateTime? ExpiresAtUtc = null);
