using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Domain.Interfaces;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Scheduling.Queries.GetPendingSlots;

public class GetPendingSlotsHandler(
    IScheduleSlotRepository scheduleSlotRepository) : IRequestHandler<GetPendingSlotsQuery, List<ScheduleSlotDto>>
{
    public async ValueTask<List<ScheduleSlotDto>> Handle(GetPendingSlotsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var slots = await ((IQueryable<PostForge.Domain.Entities.ScheduleSlot>)scheduleSlotRepository)
            .Where(s => s.Status == PostStatus.Scheduled && s.ScheduledAtUtc <= now)
            .OrderBy(s => s.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

        return slots.Select(s => s.ToDto()).ToList();
    }
}
