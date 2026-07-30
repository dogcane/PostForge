using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class AnthropicTextProvider : IAiTextProvider
{
    public string ProviderKey => "anthropic";

    public Task<string> GenerateCaptionAsync(CaptionRequestDto request, CancellationToken ct)
        => Task.FromResult($"[Anthropic placeholder] Generated caption for: {request.Brief}");
}
