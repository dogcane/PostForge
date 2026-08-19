using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class TenantMembershipRepository : TenantScopedRepository<TenantMembership, Guid>, ITenantMembershipRepository
{
    public TenantMembershipRepository(IDataContext dataContext, ITenantContext tenantContext)
        : base(dataContext, tenantContext) { }
}