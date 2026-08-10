using PostForge.Domain.ValueObjects;

namespace PostForge.Infrastructure.Providers.Social;

public class SocialPlatformProviderRegistry : ISocialPlatformProviderRegistry
{
    private readonly Dictionary<SocialPlatform, ISocialPlatformProvider> _byPlatform;
    private readonly Dictionary<string, ISocialPlatformProvider> _byIdentifier;

    public SocialPlatformProviderRegistry(IEnumerable<ISocialPlatformProvider> providers)
    {
        var providerList = providers.ToList();
        _byPlatform = providerList.ToDictionary(p => p.Platform);
        _byIdentifier = providerList.ToDictionary(p => p.Identifier, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _byIdentifier.Keys;

    public ISocialPlatformProvider Resolve(string providerKey)
        => _byIdentifier.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"Social platform provider '{providerKey}' not found. Available: {string.Join(", ", _byIdentifier.Keys)}.");

    public ISocialPlatformProvider Resolve(SocialPlatform platform)
        => _byPlatform.TryGetValue(platform, out var provider)
            ? provider
            : throw new KeyNotFoundException($"Social platform provider '{platform}' not found. Available: {string.Join(", ", _byPlatform.Keys)}.");
}
