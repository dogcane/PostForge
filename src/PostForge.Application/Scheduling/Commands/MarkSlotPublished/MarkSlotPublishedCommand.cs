using Mediator;

namespace PostForge.Application.Scheduling.Commands.MarkSlotPublished;

public record MarkSlotPublishedCommand(Guid SlotId) : IRequest<Unit>;
