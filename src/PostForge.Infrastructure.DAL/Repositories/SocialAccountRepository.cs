using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class SocialAccountRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<SocialAccount, Guid>(dataContext, tenantContext), ISocialAccountRepository;
