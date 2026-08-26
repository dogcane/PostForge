using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class ProviderCredentialRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<ProviderCredential, Guid>(dataContext, tenantContext), IProviderCredentialRepository
{
    public Task<ProviderCredential?> FindByProviderKeyAsync(string providerKey, CancellationToken cancellationToken = default)
    {
        var result = this.FirstOrDefault(c => string.Equals(c.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<ProviderCredential>> ListByScopeAsync(PostForge.Domain.ValueObjects.ProviderCredentialScope scope, CancellationToken cancellationToken = default)
    {
        var result = this.Where(c => c.Scope == scope).ToList();
        return Task.FromResult<IReadOnlyList<ProviderCredential>>(result);
    }
}
