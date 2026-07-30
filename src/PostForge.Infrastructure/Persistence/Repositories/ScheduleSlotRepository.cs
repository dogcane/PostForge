using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Infrastructure.Persistence.Repositories;

public class ScheduleSlotRepository : EntityFrameworkRepository<ScheduleSlot, Guid>, IScheduleSlotRepository
{
    public ScheduleSlotRepository(IDataContext dataContext) : base(dataContext) { }
}
