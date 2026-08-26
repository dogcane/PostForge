using Mediator;
using PostForge.Application.ProviderCredentials.DTOs;

namespace PostForge.Application.ProviderCredentials.Queries.GetAllProviderCredentials;

public record GetAllProviderCredentialsQuery : IRequest<List<ProviderCredentialDto>>;
