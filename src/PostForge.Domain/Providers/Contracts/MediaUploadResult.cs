namespace PostForge.Domain.Providers.Contracts;

public record MediaUploadResult(
    string MediaId,
    string? UploadUrl = null,
    DateTime? ExpiresAtUtc = null);