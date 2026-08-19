using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class ScheduleSlotRepository : TenantScopedRepository<ScheduleSlot, Guid>, IScheduleSlotRepository
{
    public ScheduleSlotRepository(IDataContext dataContext, ITenantContext tenantContext) : base(dataContext, tenantContext) { }
}
