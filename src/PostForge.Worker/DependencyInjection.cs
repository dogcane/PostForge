using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Messaging.ServiceBus;
using PostForge.Infrastructure.Providers;
using PostForge.Providers.Facebook;
using PostForge.Providers.Instagram;
using PostForge.Providers.TikTok;
using PostForge.Providers.YouTube;

namespace PostForge.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantContext, SystemTenantContext>();

        services.AddDataAccess(configuration);

        services.AddFacebookProvider(configuration);
        services.AddInstagramProvider();
        services.AddTikTokProvider();
        services.AddYouTubeProvider();

        services.AddProviderRegistries();
        services.AddServiceBusPublishJobSender(configuration);

        return services;
    }
}