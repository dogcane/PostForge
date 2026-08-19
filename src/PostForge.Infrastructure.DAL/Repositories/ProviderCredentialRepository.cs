using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class ProviderCredentialRepository : TenantScopedRepository<ProviderCredential, Guid>, IProviderCredentialRepository
{
    public ProviderCredentialRepository(IDataContext dataContext, ITenantContext tenantContext) : base(dataContext, tenantContext) { }
}
