using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class ProviderCredential : AggregateRoot<Guid>
{
    public Guid Id => Identity;
    public string ProviderKey { get; private set; }
    public ProviderCredentialScope Scope { get; private set; }
    public string KeyVaultReference { get; private set; }
    public bool IsValidated { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ProviderCredential() : base(Guid.NewGuid())
    {
        ProviderKey = null!;
        KeyVaultReference = null!;
    }

    private ProviderCredential(string providerKey, ProviderCredentialScope scope, string keyVaultReference) : base(Guid.NewGuid())
    {
        ProviderKey = providerKey;
        Scope = scope;
        KeyVaultReference = keyVaultReference;
        IsValidated = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<ProviderCredential> Create(string providerKey, ProviderCredentialScope scope, string keyVaultReference)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(providerKey, "ProviderKey").Required().StringLength(100)
            .With(scope, "Scope").Condition(v => Enum.IsDefined(typeof(ProviderCredentialScope), v))
            .With(keyVaultReference, "KeyVaultReference").Required().StringLength(500);
        if (!result.Success)
            return result;
        return OperationResult<ProviderCredential>.MakeSuccess(new ProviderCredential(providerKey, scope, keyVaultReference));
    }

    public OperationResult MarkAsValidated()
    {
        if (IsValidated)
            return OperationResult.MakeFailure(ErrorMessage.Create("IsValidated", "Credential is already validated."));
        IsValidated = true;
        return OperationResult.MakeSuccess();
    }
}
