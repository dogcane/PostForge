using AutoMapper;
using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;
using PostForge.Application.Scheduling.DTOs;

namespace PostForge.Application.Scheduling.Queries.GetSlotsByPostId;

public class GetSlotsByPostIdHandler(
    IScheduleSlotRepository scheduleSlotRepository,
    IMapper mapper) : IRequestHandler<GetSlotsByPostIdQuery, List<ScheduleSlotDto>>
{
    public async ValueTask<List<ScheduleSlotDto>> Handle(GetSlotsByPostIdQuery request, CancellationToken cancellationToken)
    {
        var slots = await scheduleSlotRepository
            .Where(s => s.PostId == request.PostId)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<ScheduleSlotDto>>(slots);
    }
}
