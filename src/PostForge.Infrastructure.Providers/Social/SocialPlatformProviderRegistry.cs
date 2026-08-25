using PostForge.Domain.Providers;

namespace PostForge.Infrastructure.Providers.Social;

public class SocialPlatformProviderRegistry(IEnumerable<ISocialPlatformProvider> providers) : ISocialPlatformProviderRegistry
{
    private readonly Dictionary<string, ISocialPlatformProvider> _providers = providers.ToDictionary(p => p.Identifier, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> AvailableProviderKeys => _providers.Keys;

    public ISocialPlatformProvider Resolve(string providerKey)
        => _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"Social platform provider '{providerKey}' not found. Available: {string.Join(", ", _providers.Keys)}.");
}