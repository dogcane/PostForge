using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.ProviderCredentials.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Queries.GetProviderCredentialById;

public class GetProviderCredentialByIdHandler(IProviderCredentialRepository repository) : IRequestHandler<GetProviderCredentialByIdQuery, ProviderCredentialDto?>
{
    public async ValueTask<ProviderCredentialDto?> Handle(GetProviderCredentialByIdQuery request, CancellationToken cancellationToken)
    {
        var credential = await repository.LoadAsync(request.Id);
        return credential?.ToDto();
    }
}
