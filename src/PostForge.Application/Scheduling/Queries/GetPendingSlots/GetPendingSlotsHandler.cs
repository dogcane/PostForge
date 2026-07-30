using AutoMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Scheduling.Queries.GetPendingSlots;

public class GetPendingSlotsHandler(
    IScheduleSlotRepository scheduleSlotRepository,
    IMapper mapper) : IRequestHandler<GetPendingSlotsQuery, List<ScheduleSlotDto>>
{
    public async ValueTask<List<ScheduleSlotDto>> Handle(GetPendingSlotsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var slots = await scheduleSlotRepository
            .Where(s => s.Status == PostStatus.Scheduled && s.ScheduledAtUtc <= now)
            .OrderBy(s => s.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ScheduleSlotDto>>(slots);
    }
}
