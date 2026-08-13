using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.DallE;

public class DallEImageProvider : IAiImageProvider
{
    public string ProviderKey => "dall-e";

    public Task<GeneratedImage> GenerateImageAsync(ImageRequest request, CancellationToken ct)
        => Task.FromResult(new GeneratedImage("https://placeholder.blob.core.windows.net/images/dall-e-placeholder.png", "image/png"));
}