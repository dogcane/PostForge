using Mediator;
using PostForge.Application.Scheduling.Queries.GetPendingSlots;
using PostForge.Infrastructure.Messaging;

namespace PostForge.Worker.Services;

public sealed class PublishJobService(
    ILogger<PublishJobService> logger,
    IMediator mediator,
    IPublishJobSender publishJobSender) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PublishJobService started. Polling for pending ScheduleSlots every 30 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingSlots = await mediator.Send(new GetPendingSlotsQuery(), stoppingToken);

                foreach (var slot in pendingSlots)
                {
                    logger.LogInformation(
                        "Sending slot {SlotId} (Post {PostId}, Platform {Platform}) to publish queue",
                        slot.Id, slot.PostId, slot.Platform);

                    await publishJobSender.SendPublishJobAsync(slot.Id, stoppingToken);
                }

                if (pendingSlots.Count > 0)
                {
                    logger.LogInformation("Queued {Count} pending slot(s) for publishing", pendingSlots.Count);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while polling for pending schedule slots");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        logger.LogInformation("PublishJobService stopped.");
    }
}
