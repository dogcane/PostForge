using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Commands.CreateProviderCredential;

public class CreateProviderCredentialHandler(
    IProviderCredentialRepository credentialRepository,
    IDataContext dataContext,
    ITenantContext tenantContext) : IRequestHandler<CreateProviderCredentialCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProviderCredentialCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId
            ?? throw new InvalidOperationException("A tenant context is required to create a provider credential.");

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
}
