using PostForge.Domain.Providers;

namespace PostForge.Infrastructure.Providers.Ai;

public class AiTextProviderRegistry : IProviderRegistry<IAiTextProvider>
{
    private readonly Dictionary<string, IAiTextProvider> _providers;

    public AiTextProviderRegistry(IEnumerable<IAiTextProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> AvailableProviderKeys => _providers.Keys;

    public IAiTextProvider Resolve(string providerKey)
        => _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"AI text provider '{providerKey}' not found. Available: {string.Join(", ", _providers.Keys)}.");
}
