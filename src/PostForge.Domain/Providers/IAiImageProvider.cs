using PostForge.Domain.Providers.Contracts;

namespace PostForge.Domain.Providers;

public interface IAiImageProvider
{
    string ProviderKey { get; }
    Task<GeneratedImage> GenerateImageAsync(ImageRequest request, CancellationToken ct);
}