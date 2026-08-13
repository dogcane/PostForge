using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.GoogleGemini;

public class GoogleGeminiTextProvider : IAiTextProvider
{
    public string ProviderKey => "google-gemini";

    public Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct)
        => Task.FromResult($"[Google Gemini placeholder] Generated caption for: {request.Brief}");
}