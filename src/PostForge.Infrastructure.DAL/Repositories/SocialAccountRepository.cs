using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class SocialAccountRepository : TenantScopedRepository<SocialAccount, Guid>, ISocialAccountRepository
{
    public SocialAccountRepository(IDataContext dataContext, ITenantContext tenantContext) : base(dataContext, tenantContext) { }
}
