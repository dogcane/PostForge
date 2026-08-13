using PostForge.Domain.Providers;

namespace PostForge.Infrastructure.Providers.Social;

public class SocialPlatformProviderRegistry : ISocialPlatformProviderRegistry
{
    private readonly Dictionary<string, ISocialPlatformProvider> _byIdentifier;

    public SocialPlatformProviderRegistry(IEnumerable<ISocialPlatformProvider> providers)
    {
        _byIdentifier = providers.ToDictionary(p => p.Identifier, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _byIdentifier.Keys;

    public ISocialPlatformProvider Resolve(string providerKey)
        => _byIdentifier.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"Social platform provider '{providerKey}' not found. Available: {string.Join(", ", _byIdentifier.Keys)}.");
}