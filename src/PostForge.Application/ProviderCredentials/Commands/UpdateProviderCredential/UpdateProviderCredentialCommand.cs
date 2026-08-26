using Mediator;

namespace PostForge.Application.ProviderCredentials.Commands.UpdateProviderCredential;

public record UpdateProviderCredentialCommand(
    Guid Id,
    string DisplayName,
    string? Description,
    string? KeyVaultReference,
    string? SecretValue,
    string? SettingsJson,
    bool IsEnabled) : IRequest;
