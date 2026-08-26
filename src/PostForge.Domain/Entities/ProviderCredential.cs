using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class ProviderCredential : AggregateRoot<Guid>
{
    #region Fields
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string ProviderKey { get; private set; }
    public ProviderCredentialScope Scope { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public string? KeyVaultReference { get; private set; }
    public string? SecretValue { get; private set; }
    public string? SettingsJson { get; private set; }
    public bool IsEnabled { get; private set; }
    public bool IsValidated { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    #endregion

    #region ctor
    private ProviderCredential() : base(Guid.NewGuid())
    {
        ProviderKey = null!;
        DisplayName = null!;
    }

    private ProviderCredential(
        Guid tenantId,
        string providerKey,
        ProviderCredentialScope scope,
        string displayName,
        string? description,
        string? keyVaultReference,
        string? secretValue,
        string? settingsJson,
        bool isEnabled) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        ProviderKey = providerKey;
        Scope = scope;
        DisplayName = displayName;
        Description = description;
        KeyVaultReference = keyVaultReference;
        SecretValue = secretValue;
        SettingsJson = settingsJson;
        IsEnabled = isEnabled;
        IsValidated = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(
        Guid tenantId,
        string providerKey,
        ProviderCredentialScope scope,
        string displayName,
        string? keyVaultReference,
        string? secretValue,
        string? settingsJson)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(providerKey, "ProviderKey").Required().StringLength(100)
            .With(scope, "Scope").Condition(v => Enum.IsDefined(v))
            .With(displayName, "DisplayName").Required().StringLength(200);
        if (!string.IsNullOrWhiteSpace(keyVaultReference))
            result.With(keyVaultReference!, "KeyVaultReference").StringLength(500);
        if (!string.IsNullOrWhiteSpace(secretValue))
            result.With(secretValue!, "SecretValue").StringLength(2000);
        if (!string.IsNullOrWhiteSpace(settingsJson))
            result.With(settingsJson!, "SettingsJson").StringLength(4000);
        return result;
    }

    public static OperationResult<ProviderCredential> Create(
        Guid tenantId,
        string providerKey,
        ProviderCredentialScope scope,
        string keyVaultReference)
        => Create(tenantId, providerKey, scope, providerKey, null, keyVaultReference, null, null, true);

    public static OperationResult<ProviderCredential> Create(
        Guid tenantId,
        string providerKey,
        ProviderCredentialScope scope,
        string displayName,
        string? description,
        string? keyVaultReference,
        string? secretValue,
        string? settingsJson,
        bool isEnabled = true)
        => Validate(tenantId, providerKey, scope, displayName, keyVaultReference, secretValue, settingsJson)
            .IfSuccessThenReturn<ProviderCredential>(() => new ProviderCredential(tenantId, providerKey, scope, displayName, description, keyVaultReference, secretValue, settingsJson, isEnabled));

    public OperationResult UpdateDetails(string displayName, string? description, string? keyVaultReference, string? settingsJson, bool isEnabled)
        => OperationResult.MakeSuccess()
            .With(displayName, "DisplayName").Required().StringLength(200)
            .Result
            .IfSuccess(_ =>
            {
                DisplayName = displayName;
                Description = description;
                KeyVaultReference = string.IsNullOrWhiteSpace(keyVaultReference) ? null : keyVaultReference;
                SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? null : settingsJson;
                IsEnabled = isEnabled;
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult UpdateSecret(string? secretValue)
        => string.IsNullOrWhiteSpace(secretValue)
            ? OperationResult.MakeFailure(ErrorMessage.Create("SecretValue", "Secret value is required."))
            : OperationResult.MakeSuccess()
                .With(secretValue!, "SecretValue").StringLength(2000)
                .Result
                .IfSuccess(_ =>
                {
                    SecretValue = secretValue;
                    IsValidated = false;
                    UpdatedAtUtc = DateTime.UtcNow;
                });

    public OperationResult ClearSecret()
        => OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            SecretValue = null;
            IsValidated = false;
            UpdatedAtUtc = DateTime.UtcNow;
        });

    public OperationResult UpdateSettings(string? settingsJson)
        => OperationResult.MakeSuccess()
            .With(settingsJson ?? string.Empty, "SettingsJson").Condition(v => v != null && v.Length <= 4000)
            .Result
            .IfSuccess(_ =>
            {
                SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? null : settingsJson;
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult SetEnabled(bool enabled)
        => OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            IsEnabled = enabled;
            UpdatedAtUtc = DateTime.UtcNow;
        });

    public OperationResult MarkAsValidated()
        => IsValidated
            ? OperationResult.MakeFailure(ErrorMessage.Create("IsValidated", "Credential is already validated."))
            : OperationResult.MakeSuccess().IfSuccess(_ =>
            {
                IsValidated = true;
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult ResetValidation()
        => OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            IsValidated = false;
            UpdatedAtUtc = DateTime.UtcNow;
        });

    public string? GetMaskedSecret()
    {
        if (string.IsNullOrWhiteSpace(SecretValue))
            return null;
        var secret = SecretValue!;
        return $"{secret[..Math.Min(4, secret.Length)]}****";
    }
    #endregion
}
