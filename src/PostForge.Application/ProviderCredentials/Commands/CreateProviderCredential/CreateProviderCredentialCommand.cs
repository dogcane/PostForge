using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.ProviderCredentials.Commands.CreateProviderCredential;

public record CreateProviderCredentialCommand(
    string ProviderKey,
    ProviderCredentialScope Scope,
    string DisplayName,
    string? Description,
    string? KeyVaultReference,
    string? SecretValue,
    string? SettingsJson,
    bool IsEnabled) : IRequest<Guid>;
