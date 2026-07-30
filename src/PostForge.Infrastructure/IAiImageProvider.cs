using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure;

public interface IAiImageProvider
{
    string ProviderKey { get; }
    Task<GeneratedImageDto> GenerateImageAsync(ImageRequestDto request, CancellationToken ct);
}
