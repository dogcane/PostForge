using PostForge.Domain.ValueObjects;

namespace PostForge.Application.ProviderCredentials.DTOs;

public class ProviderCredentialDto
{
    public Guid Id { get; set; }
    public string ProviderKey { get; set; } = null!;
    public ProviderCredentialScope Scope { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string? KeyVaultReference { get; set; }
    public string? MaskedSecret { get; set; }
    public bool HasSecret { get; set; }
    public string? SettingsJson { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsValidated { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class CreateProviderCredentialRequest
{
    public string ProviderKey { get; set; } = null!;
    public ProviderCredentialScope Scope { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string? KeyVaultReference { get; set; }
    public string? SecretValue { get; set; }
    public string? SettingsJson { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateProviderCredentialRequest
{
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string? KeyVaultReference { get; set; }
    public string? SecretValue { get; set; }
    public string? SettingsJson { get; set; }
    public bool IsEnabled { get; set; } = true;
}
