using ECO;
using PostForge.Domain.Entities;

namespace PostForge.Domain.Interfaces;

public interface IProviderCredentialRepository : IRepository<ProviderCredential, Guid>
{
    Task<ProviderCredential?> FindByProviderKeyAsync(string providerKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProviderCredential>> ListByScopeAsync(ValueObjects.ProviderCredentialScope scope, CancellationToken cancellationToken = default);
}
