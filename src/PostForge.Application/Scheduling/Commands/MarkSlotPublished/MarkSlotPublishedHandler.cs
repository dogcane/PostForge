using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Scheduling.Commands.MarkSlotPublished;

public class MarkSlotPublishedHandler(
    IScheduleSlotRepository scheduleSlotRepository,
    IDataContext dataContext) : IRequestHandler<MarkSlotPublishedCommand, Unit>
{
    public async ValueTask<Unit> Handle(MarkSlotPublishedCommand request, CancellationToken cancellationToken)
    {
        var slot = await scheduleSlotRepository.LoadAsync(request.SlotId)
            ?? throw new KeyNotFoundException($"ScheduleSlot with Id {request.SlotId} was not found.");

        var result = slot.MarkPublished();
        if (!result.Success)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));

        scheduleSlotRepository.Update(slot);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
