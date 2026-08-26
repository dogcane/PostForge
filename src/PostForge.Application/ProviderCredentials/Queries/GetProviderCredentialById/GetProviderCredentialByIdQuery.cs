using Mediator;
using PostForge.Application.ProviderCredentials.DTOs;

namespace PostForge.Application.ProviderCredentials.Queries.GetProviderCredentialById;

public record GetProviderCredentialByIdQuery(Guid Id) : IRequest<ProviderCredentialDto?>;
