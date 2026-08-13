using Mediator;

namespace PostForge.Application.Scheduling.Commands.SchedulePost;

public record SchedulePostCommand(
    Guid PostId,
    string Platform,
    DateTime ScheduledAtUtc) : IRequest<Guid>;
