namespace PostForge.Domain.Providers;

public interface IProviderRegistry<TProvider>
{
    TProvider Resolve(string providerKey);
    IReadOnlyCollection<string> AvailableProviderKeys { get; }
}