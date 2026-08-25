using ECO.Data;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class ScheduleSlotRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<ScheduleSlot, Guid>(dataContext, tenantContext), IScheduleSlotRepository;
