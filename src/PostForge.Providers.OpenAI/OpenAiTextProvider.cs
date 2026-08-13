using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.OpenAI;

public class OpenAiTextProvider : IAiTextProvider
{
    public string ProviderKey => "openai";

    public Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct)
        => Task.FromResult($"[OpenAI placeholder] Generated caption for: {request.Brief}");
}