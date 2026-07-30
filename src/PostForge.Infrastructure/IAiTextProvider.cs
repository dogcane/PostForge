using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure;

public interface IAiTextProvider
{
    string ProviderKey { get; }
    Task<string> GenerateCaptionAsync(CaptionRequestDto request, CancellationToken ct);
}
