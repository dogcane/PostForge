using PostForge.Worker.Jobs;
using Quartz;

namespace PostForge.Worker.Services;

internal sealed class QuartzHostedService(IScheduler scheduler, ILogger<QuartzHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Quartz scheduler");

        var job = JobBuilder.Create<PublishPostJob>()
            .WithIdentity("PublishPostJob", "default")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("PublishPostTrigger", "default")
            .StartNow()
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInMinutes(1)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
        await scheduler.Start(cancellationToken);

        logger.LogInformation("Quartz scheduler started. PublishPostJob will run every minute.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Shutting down Quartz scheduler");

        if (scheduler.IsStarted)
        {
            await scheduler.Shutdown(cancellationToken);
        }
    }
}
