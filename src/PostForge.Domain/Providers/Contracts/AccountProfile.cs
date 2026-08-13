namespace PostForge.Domain.Providers.Contracts;

public record AccountProfile(
    string ExternalId,
    string DisplayName,
    string? AvatarUrl = null,
    string? Username = null,
    long? FollowerCount = null);