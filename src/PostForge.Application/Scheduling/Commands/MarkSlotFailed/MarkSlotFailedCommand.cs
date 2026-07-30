using Mediator;

namespace PostForge.Application.Scheduling.Commands.MarkSlotFailed;

public record MarkSlotFailedCommand(Guid SlotId, string Error) : IRequest<Unit>;
