using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class DallEImageProvider : IAiImageProvider
{
    public string ProviderKey => "dall-e";

    public Task<GeneratedImageDto> GenerateImageAsync(ImageRequestDto request, CancellationToken ct)
        => Task.FromResult(new GeneratedImageDto("https://placeholder.blob.core.windows.net/images/dall-e-placeholder.png", "image/png"));
}
