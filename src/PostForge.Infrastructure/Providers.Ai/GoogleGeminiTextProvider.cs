using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class GoogleGeminiTextProvider : IAiTextProvider
{
    public string ProviderKey => "google-gemini";

    public Task<string> GenerateCaptionAsync(CaptionRequestDto request, CancellationToken ct)
        => Task.FromResult($"[Google Gemini placeholder] Generated caption for: {request.Brief}");
}
