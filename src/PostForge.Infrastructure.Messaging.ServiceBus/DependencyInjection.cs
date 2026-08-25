using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostForge.Infrastructure.Messaging;

namespace PostForge.Infrastructure.Messaging.ServiceBus;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceBusPublishJobSender(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
        {
            services.AddScoped<IPublishJobSender, NoOpPublishJobSender>();
            return services;
        }

        services.AddSingleton(sp =>
        {
            var client = new ServiceBusClient(serviceBusConnectionString);
            return client.CreateSender("publish-jobs");
        });

        services.AddScoped<IPublishJobSender, ServiceBusPublishJobSender>();

        return services;
    }

    private sealed class NoOpPublishJobSender(ILogger<NoOpPublishJobSender> logger) : IPublishJobSender
    {
        public Task SendPublishJobAsync(Guid slotId, CancellationToken ct)
        {
            logger.LogWarning("ServiceBus connection string not configured — skipping publish job for slot {SlotId}", slotId);
            return Task.CompletedTask;
        }
    }
}