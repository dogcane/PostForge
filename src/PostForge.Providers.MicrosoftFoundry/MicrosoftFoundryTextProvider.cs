using PostForge.Domain.Providers;
using PostForge.Domain.Providers.Contracts;

namespace PostForge.Providers.MicrosoftFoundry;

public class MicrosoftFoundryTextProvider : IAiTextProvider
{
    public string ProviderKey => "microsoft-foundry";

    public Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct)
        => Task.FromResult($"[Microsoft Foundry placeholder] Generated caption for: {request.Brief}");
}