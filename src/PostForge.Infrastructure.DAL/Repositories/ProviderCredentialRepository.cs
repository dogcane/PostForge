using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class ProviderCredentialRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<ProviderCredential, Guid>(dataContext, tenantContext), IProviderCredentialRepository;
