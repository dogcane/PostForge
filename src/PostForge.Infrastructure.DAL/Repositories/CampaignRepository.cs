using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class CampaignRepository : TenantScopedRepository<Campaign, Guid>, ICampaignRepository
{
    public CampaignRepository(IDataContext dataContext, ITenantContext tenantContext) : base(dataContext, tenantContext) { }
}
