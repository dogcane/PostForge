using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Scheduling.Queries.GetCalendarSlots;

public class GetCalendarSlotsHandler(
    IScheduleSlotRepository scheduleSlotRepository) : IRequestHandler<GetCalendarSlotsQuery, List<ScheduleSlotDto>>
{
    public async ValueTask<List<ScheduleSlotDto>> Handle(GetCalendarSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await ((IQueryable<PostForge.Domain.Entities.ScheduleSlot>)scheduleSlotRepository)
            .Where(s => s.ScheduledAtUtc >= request.StartUtc && s.ScheduledAtUtc < request.EndUtc)
            .OrderBy(s => s.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

        return slots.Select(s => s.ToDto()).ToList();
    }
}