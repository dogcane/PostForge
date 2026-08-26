using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Commands.UpdateProviderCredential;

public class UpdateProviderCredentialHandler(
    IProviderCredentialRepository credentialRepository,
    IDataContext dataContext) : IRequestHandler<UpdateProviderCredentialCommand>
{
    public async ValueTask<Unit> Handle(UpdateProviderCredentialCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Provider credential '{request.Id}' not found.");

        credential.UpdateDetails(request.DisplayName, request.Description, request.KeyVaultReference, request.SettingsJson, request.IsEnabled).EnsureSuccess();

        if (request.SecretValue is not null)
        {
            if (string.IsNullOrWhiteSpace(request.SecretValue))
                credential.ClearSecret().EnsureSuccess();
            else
                credential.UpdateSecret(request.SecretValue).EnsureSuccess();
        }

        await dataContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
