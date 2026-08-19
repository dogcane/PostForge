using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
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

        slot.MarkFailed(request.Error).EnsureSuccess();

        scheduleSlotRepository.Update(slot);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
