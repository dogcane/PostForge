namespace PostForge.Infrastructure;

public interface IProviderRegistry<TProvider>
{
    TProvider Resolve(string providerKey);
    IReadOnlyCollection<string> AvailableProviderKeys { get; }
}
