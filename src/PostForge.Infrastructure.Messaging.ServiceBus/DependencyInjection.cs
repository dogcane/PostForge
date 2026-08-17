using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Infrastructure.Messaging;

namespace PostForge.Infrastructure.Messaging.ServiceBus;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceBusPublishJobSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var serviceBusConnectionString = config.GetConnectionString("ServiceBus")
                ?? throw new InvalidOperationException("Connection string 'ServiceBus' not found.");

            var client = new ServiceBusClient(serviceBusConnectionString);
            return client.CreateSender("publish-jobs");
        });

        services.AddScoped<IPublishJobSender, ServiceBusPublishJobSender>();

        return services;
    }
}