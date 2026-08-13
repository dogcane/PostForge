using Mediator;
using PostForge.Application.Scheduling.Commands.MarkSlotPublished;
using PostForge.Application.Scheduling.Queries.GetPendingSlots;
using PostForge.Domain.Providers;
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
            // Stub: resolves the social platform provider for the target platform and
            // logs its metadata/capabilities. The actual PublishAsync call will be
            // wired in Phase 1 once post content and OAuth tokens are loaded.
            var registry = scope.ServiceProvider.GetRequiredService<ISocialPlatformProviderRegistry>();
            foreach (var provider in registry.AvailableProviderKeys.Select(registry.Resolve))
            {
                logger.LogInformation(
                    "Provider available: {ProviderName} ({ProviderIdentifier}) for platform {Platform}, capabilities {Capabilities}",
                    provider.Name, provider.Identifier, provider.Identifier, provider.Capabilities);
            }

            await mediator.Send(new MarkSlotPublishedCommand(slotId), context.CancellationToken);
            logger.LogInformation("Successfully marked slot {SlotId} as published", slotId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish slot {SlotId}", slotId);
        }
    }
}
