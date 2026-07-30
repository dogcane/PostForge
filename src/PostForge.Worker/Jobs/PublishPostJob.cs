using Mediator;
using PostForge.Application.Scheduling.Commands.MarkSlotPublished;
using PostForge.Application.Scheduling.Queries.GetPendingSlots;
using Quartz;

namespace PostForge.Worker.Jobs;

public sealed class PublishPostJob(
    ILogger<PublishPostJob> logger,
    IServiceScopeFactory scopeFactory) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        Guid slotId;

        if (context.MergedJobDataMap.ContainsKey("ScheduleSlotId"))
        {
            slotId = context.MergedJobDataMap.GetGuid("ScheduleSlotId");
            logger.LogInformation("Processing publish job for slot {SlotId} from JobDataMap", slotId);
        }
        else
        {
            var pendingSlots = await mediator.Send(new GetPendingSlotsQuery(), context.CancellationToken);
            var slot = pendingSlots.FirstOrDefault();

            if (slot is null)
            {
                logger.LogDebug("No pending schedule slots found");
                return;
            }

            slotId = slot.Id;
            logger.LogInformation(
                "Processing publish job for next pending slot {SlotId} (Post {PostId})",
                slot.Id, slot.PostId);
        }

        try
        {
            // Stub: in real implementation would resolve ISocialPlatformProvider
            // and call PublishAsync before marking as published.
            await mediator.Send(new MarkSlotPublishedCommand(slotId), context.CancellationToken);
            logger.LogInformation("Successfully marked slot {SlotId} as published", slotId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish slot {SlotId}", slotId);
        }
    }
}
