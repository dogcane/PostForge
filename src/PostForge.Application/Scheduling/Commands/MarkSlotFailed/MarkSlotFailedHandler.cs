using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Scheduling.Commands.MarkSlotFailed;

public class MarkSlotFailedHandler(
    IScheduleSlotRepository scheduleSlotRepository,
    IDataContext dataContext) : IRequestHandler<MarkSlotFailedCommand, Unit>
{
    public async ValueTask<Unit> Handle(MarkSlotFailedCommand request, CancellationToken cancellationToken)
    {
        var slot = await scheduleSlotRepository.LoadAsync(request.SlotId)
            ?? throw new KeyNotFoundException($"ScheduleSlot with Id {request.SlotId} was not found.");

        var result = slot.MarkFailed(request.Error);
        if (!result.Success)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));

        scheduleSlotRepository.Update(slot);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
