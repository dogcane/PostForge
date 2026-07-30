using Mediator;
using PostForge.Application;
using PostForge.Infrastructure;
using PostForge.Worker.Jobs;
using PostForge.Worker.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddApplication();
    services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
    services.AddInfrastructure(hostContext.Configuration);

    services.AddSingleton<IJobFactory, PublishPostJobFactory>();
    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

    services.AddScoped<PublishPostJob>();

    services.AddSingleton(provider =>
    {
        var factory = provider.GetRequiredService<ISchedulerFactory>();
        var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
        scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
        return scheduler;
    });

    services.AddHostedService<PublishJobService>();
    services.AddHostedService<QuartzHostedService>();
});

builder.ConfigureLogging(logging =>
{
    logging.AddJsonConsole();
});

var host = builder.Build();
await host.RunAsync();

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
