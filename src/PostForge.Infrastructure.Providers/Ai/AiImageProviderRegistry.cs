using PostForge.Domain.Providers;

namespace PostForge.Infrastructure.Providers.Ai;

public class AiImageProviderRegistry(IEnumerable<IAiImageProvider> providers) : IProviderRegistry<IAiImageProvider>
{
    private readonly Dictionary<string, IAiImageProvider> _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> AvailableProviderKeys => _providers.Keys;

    public IAiImageProvider Resolve(string providerKey)
        => _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"AI image provider '{providerKey}' not found. Available: {string.Join(", ", _providers.Keys)}.");
}