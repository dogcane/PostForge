using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Commands.DeleteProviderCredential;

public class DeleteProviderCredentialHandler(
    IProviderCredentialRepository credentialRepository,
    IDataContext dataContext) : IRequestHandler<DeleteProviderCredentialCommand>
{
    public async ValueTask<Unit> Handle(DeleteProviderCredentialCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Provider credential '{request.Id}' not found.");

        credentialRepository.Remove(credential);
        await dataContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
