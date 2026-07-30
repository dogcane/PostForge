using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Infrastructure.Persistence.Repositories;

public class CampaignRepository : EntityFrameworkRepository<Campaign, Guid>, ICampaignRepository
{
    public CampaignRepository(IDataContext dataContext) : base(dataContext) { }
}
