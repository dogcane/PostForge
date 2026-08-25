using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class CampaignRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<Campaign, Guid>(dataContext, tenantContext), ICampaignRepository;
