using Mediator;
using PostForge.Application.Scheduling.DTOs;

namespace PostForge.Application.Scheduling.Queries.GetSlotsByPostId;

public record GetSlotsByPostIdQuery(Guid PostId) : IRequest<List<ScheduleSlotDto>>;
