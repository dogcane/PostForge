using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.ProviderCredentials.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.ProviderCredentials.Queries.GetAllProviderCredentials;

public class GetAllProviderCredentialsHandler(IProviderCredentialRepository repository) : IRequestHandler<GetAllProviderCredentialsQuery, List<ProviderCredentialDto>>
{
    public ValueTask<List<ProviderCredentialDto>> Handle(GetAllProviderCredentialsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ProviderCredential> query = repository;
        var credentials = query.ToList();
        return ValueTask.FromResult(credentials.Select(c => c.ToDto()).ToList());
    }
}
