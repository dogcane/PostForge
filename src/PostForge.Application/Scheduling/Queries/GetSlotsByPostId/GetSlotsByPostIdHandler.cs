using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Scheduling.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Scheduling.Queries.GetSlotsByPostId;

public class GetSlotsByPostIdHandler(
    IScheduleSlotRepository scheduleSlotRepository) : IRequestHandler<GetSlotsByPostIdQuery, List<ScheduleSlotDto>>
{
    public async ValueTask<List<ScheduleSlotDto>> Handle(GetSlotsByPostIdQuery request, CancellationToken cancellationToken)
    {
        var slots = await ((IQueryable<PostForge.Domain.Entities.ScheduleSlot>)scheduleSlotRepository)
            .Where(s => s.PostId == request.PostId)
            .ToListAsync(cancellationToken);

        return slots.Select(s => s.ToDto()).ToList();
    }
}
