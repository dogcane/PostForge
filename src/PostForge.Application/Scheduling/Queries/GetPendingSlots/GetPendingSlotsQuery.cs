using Mediator;
using PostForge.Application.Scheduling.DTOs;

namespace PostForge.Application.Scheduling.Queries.GetPendingSlots;

public record GetPendingSlotsQuery() : IRequest<List<ScheduleSlotDto>>;
