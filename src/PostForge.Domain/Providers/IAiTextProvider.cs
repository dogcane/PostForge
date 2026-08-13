using PostForge.Domain.Providers.Contracts;

namespace PostForge.Domain.Providers;

public interface IAiTextProvider
{
    string ProviderKey { get; }
    Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct);
}