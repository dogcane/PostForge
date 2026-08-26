using Mediator;

namespace PostForge.Application.ProviderCredentials.Commands.DeleteProviderCredential;

public record DeleteProviderCredentialCommand(Guid Id) : IRequest;
