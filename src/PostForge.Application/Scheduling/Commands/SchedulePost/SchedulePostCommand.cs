using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Scheduling.Commands.SchedulePost;

public record SchedulePostCommand(
    Guid PostId,
    SocialPlatform Platform,
    DateTime ScheduledAtUtc) : IRequest<Guid>;
