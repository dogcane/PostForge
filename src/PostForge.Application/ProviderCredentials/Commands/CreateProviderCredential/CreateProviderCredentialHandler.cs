using ECO.Data;
using Mediator;
using PostForge.Application.Common.Exceptions;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Providers;
using PostForge.Domain.ValueObjects;
using Resulz;

namespace PostForge.Application.ProviderCredentials.Commands.CreateProviderCredential;

public class CreateProviderCredentialHandler(
    IProviderCredentialRepository credentialRepository,
    IDataContext dataContext,
    ITenantContext tenantContext,
    ISocialPlatformProviderRegistry? socialRegistry = null,
    IProviderRegistry<IAiTextProvider>? aiTextRegistry = null,
    IProviderRegistry<IAiImageProvider>? aiImageRegistry = null) : IRequestHandler<CreateProviderCredentialCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProviderCredentialCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId
            ?? throw new InvalidOperationException("A tenant context is required to create a provider credential.");

        ValidateProviderIsInstalled(request.ProviderKey, request.Scope);

        var existing = await credentialRepository.FindByProviderKeyAsync(request.ProviderKey, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"A credential for provider '{request.ProviderKey}' already exists for this tenant.");

        var credential = ProviderCredential.Create(
            tenantId,
            request.ProviderKey,
            request.Scope,
            request.DisplayName,
            request.Description,
            request.KeyVaultReference,
            request.SecretValue,
            request.SettingsJson,
            request.IsEnabled).EnsureSuccess();

        credentialRepository.Add(credential);
        await dataContext.SaveChangesAsync(cancellationToken);
        return credential.Id;
    }

    private void ValidateProviderIsInstalled(string providerKey, ProviderCredentialScope scope)
    {
        // Registries are optional in tests (HandlerTestBase doesn't register providers); skip validation if not available
        if (socialRegistry is null && aiTextRegistry is null && aiImageRegistry is null)
            return;

        var isSocial = socialRegistry?.AvailableProviderKeys.Contains(providerKey, StringComparer.OrdinalIgnoreCase) ?? false;
        var isAiText = aiTextRegistry?.AvailableProviderKeys.Contains(providerKey, StringComparer.OrdinalIgnoreCase) ?? false;
        var isAiImage = aiImageRegistry?.AvailableProviderKeys.Contains(providerKey, StringComparer.OrdinalIgnoreCase) ?? false;

        if (!isSocial && !isAiText && !isAiImage)
        {
            var available = (socialRegistry?.AvailableProviderKeys ?? [])
                .Concat(aiTextRegistry?.AvailableProviderKeys ?? [])
                .Concat(aiImageRegistry?.AvailableProviderKeys ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var availableStr = available.Count > 0 ? string.Join(", ", available) : "(none)";
            throw new DomainValidationException([
                ErrorMessage.Create("ProviderKey", $"Provider '{providerKey}' is not installed. Available providers: {availableStr}.")
            ]);
        }

        var validScopes = new List<ProviderCredentialScope>();
        if (isSocial) validScopes.Add(ProviderCredentialScope.Social);
        if (isAiText) validScopes.Add(ProviderCredentialScope.AiText);
        if (isAiImage) validScopes.Add(ProviderCredentialScope.AiImage);

        if (!validScopes.Contains(scope))
            throw new DomainValidationException([
                ErrorMessage.Create("Scope", $"Provider '{providerKey}' expects scope '{string.Join(" or ", validScopes)}' but received '{scope}'. Provider key type cannot be changed.")
            ]);
    }
}
