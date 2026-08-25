using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class TenantMembershipRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<TenantMembership, Guid>(dataContext, tenantContext), ITenantMembershipRepository;