using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class StableDiffusionImageProvider : IAiImageProvider
{
    public string ProviderKey => "stable-diffusion";

    public Task<GeneratedImageDto> GenerateImageAsync(ImageRequestDto request, CancellationToken ct)
        => Task.FromResult(new GeneratedImageDto("https://placeholder.blob.core.windows.net/images/sd-placeholder.png", "image/png"));
}
