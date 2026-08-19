using PostForge.Worker.Jobs;
using Quartz;

namespace PostForge.Worker.Services;

internal sealed class QuartzHostedService : IHostedService
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<QuartzHostedService> _logger;

    public QuartzHostedService(IScheduler scheduler, ILogger<QuartzHostedService> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Quartz scheduler");

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

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        await _scheduler.Start(cancellationToken);

        _logger.LogInformation("Quartz scheduler started. PublishPostJob will run every minute.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Quartz scheduler");

        if (_scheduler.IsStarted)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
