using PostForge.Domain.Providers;

namespace PostForge.Infrastructure.Providers.Ai;

public class AiImageProviderRegistry : IProviderRegistry<IAiImageProvider>
{
    private readonly Dictionary<string, IAiImageProvider> _providers;

    public AiImageProviderRegistry(IEnumerable<IAiImageProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _providers.Keys;

    public IAiImageProvider Resolve(string providerKey)
        => _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"AI image provider '{providerKey}' not found. Available: {string.Join(", ", _providers.Keys)}.");
}