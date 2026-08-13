namespace PostForge.Domain.Providers.Contracts;

public record PostContent(string Text, IReadOnlyList<string> MediaUrls, string PlatformIdentifier);