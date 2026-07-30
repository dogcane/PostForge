using Quartz;
using Quartz.Spi;

namespace PostForge.Worker.Jobs;

public sealed class PublishPostJobFactory(IServiceProvider serviceProvider) : IJobFactory
{
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return serviceProvider.GetRequiredService<PublishPostJob>();
    }

    public void ReturnJob(IJob job)
    {
        if (job is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
