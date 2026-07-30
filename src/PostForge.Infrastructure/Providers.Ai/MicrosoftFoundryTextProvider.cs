using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Ai;

public class MicrosoftFoundryTextProvider : IAiTextProvider
{
    public string ProviderKey => "microsoft-foundry";

    public Task<string> GenerateCaptionAsync(CaptionRequestDto request, CancellationToken ct)
        => Task.FromResult($"[Microsoft Foundry placeholder] Generated caption for: {request.Brief}");
}
