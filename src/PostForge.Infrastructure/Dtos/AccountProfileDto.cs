namespace PostForge.Infrastructure.Dtos;

public record AccountProfileDto(
    string ExternalId,
    string DisplayName,
    string? AvatarUrl = null,
    string? Username = null,
    long? FollowerCount = null);
