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
    public string KeyVaultReference { get; private set; }
    public bool IsValidated { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    #endregion

    #region ctor
    private ProviderCredential() : base(Guid.NewGuid())
    {
        ProviderKey = null!;
        KeyVaultReference = null!;
    }

    private ProviderCredential(Guid tenantId, string providerKey, ProviderCredentialScope scope, string keyVaultReference) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        ProviderKey = providerKey;
        Scope = scope;
        KeyVaultReference = keyVaultReference;
        IsValidated = false;
        CreatedAtUtc = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(Guid tenantId, string providerKey, ProviderCredentialScope scope, string keyVaultReference)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(providerKey, "ProviderKey").Required().StringLength(100)
            .With(scope, "Scope").Condition(v => Enum.IsDefined(v))
            .With(keyVaultReference, "KeyVaultReference").Required().StringLength(500);
        return result;
    }

    public static OperationResult<ProviderCredential> Create(Guid tenantId, string providerKey, ProviderCredentialScope scope, string keyVaultReference)
        => Validate(tenantId, providerKey, scope, keyVaultReference)
            .IfSuccessThenReturn<ProviderCredential>(() => new ProviderCredential(tenantId, providerKey, scope, keyVaultReference));

    public OperationResult MarkAsValidated()
        => IsValidated
            ? OperationResult.MakeFailure(ErrorMessage.Create("IsValidated", "Credential is already validated."))
            : OperationResult.MakeSuccess().IfSuccess(_ => IsValidated = true);
    #endregion
}
