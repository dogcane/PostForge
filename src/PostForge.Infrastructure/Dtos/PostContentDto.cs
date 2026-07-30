using PostForge.Domain.ValueObjects;

namespace PostForge.Infrastructure.Dtos;

public record PostContentDto(string Text, IReadOnlyList<string> MediaUrls, SocialPlatform Platform);
