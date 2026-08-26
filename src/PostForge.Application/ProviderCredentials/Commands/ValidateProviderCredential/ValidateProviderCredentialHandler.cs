using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Commands.ValidateProviderCredential;

public class ValidateProviderCredentialHandler(
    IProviderCredentialRepository credentialRepository,
    IDataContext dataContext) : IRequestHandler<ValidateProviderCredentialCommand>
{
    public async ValueTask<Unit> Handle(ValidateProviderCredentialCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Provider credential '{request.Id}' not found.");

        credential.MarkAsValidated().EnsureSuccess();
        await dataContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
