using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.Anthropic;

public class AnthropicTextProvider : IAiTextProvider
{
    public string ProviderKey => "anthropic";

    public Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct)
        => Task.FromResult($"[Anthropic placeholder] Generated caption for: {request.Brief}");
}