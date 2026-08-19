using Mediator;
using PostForge.Application;
using PostForge.Worker;
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
    services.AddWorkerInfrastructure(hostContext.Configuration);

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
