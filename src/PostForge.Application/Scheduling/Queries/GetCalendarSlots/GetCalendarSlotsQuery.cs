using Mediator;
using PostForge.Application.Scheduling.DTOs;

namespace PostForge.Application.Scheduling.Queries.GetCalendarSlots;

public record GetCalendarSlotsQuery(
    DateTime StartUtc,
    DateTime EndUtc) : IRequest<List<ScheduleSlotDto>>;