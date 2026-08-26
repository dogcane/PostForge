using System.Collections.Concurrent;
using Quartz;
using Quartz.Spi;

namespace PostForge.Worker.Jobs;

public sealed class PublishPostJobFactory(IServiceScopeFactory scopeFactory) : IJobFactory
{
    private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new();

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = scopeFactory.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<PublishPostJob>();
        _scopes[job] = scope;
        return job;
    }

    public void ReturnJob(IJob job)
    {
        if (_scopes.TryRemove(job, out var scope))
        {
            scope.Dispose();
        }

        if (job is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
