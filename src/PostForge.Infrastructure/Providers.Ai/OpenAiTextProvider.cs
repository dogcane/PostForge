using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class OpenAiTextProvider : IAiTextProvider
{
    public string ProviderKey => "openai";

    public Task<string> GenerateCaptionAsync(CaptionRequestDto request, CancellationToken ct)
        => Task.FromResult($"[OpenAI placeholder] Generated caption for: {request.Brief}");
}
