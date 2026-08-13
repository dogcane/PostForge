using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.StableDiffusion;

public class StableDiffusionImageProvider : IAiImageProvider
{
    public string ProviderKey => "stable-diffusion";

    public Task<GeneratedImage> GenerateImageAsync(ImageRequest request, CancellationToken ct)
        => Task.FromResult(new GeneratedImage("https://placeholder.blob.core.windows.net/images/sd-placeholder.png", "image/png"));
}