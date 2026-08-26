using Mediator;

namespace PostForge.Application.ProviderCredentials.Commands.ValidateProviderCredential;

public record ValidateProviderCredentialCommand(Guid Id) : IRequest;
