using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Identity;
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
        services.AddDataAccess(configuration);
        services.AddIdentityCoreInfrastructure(configuration);

        // Worker runs as system — overrides Api's tenant-aware context with a null-tenant context
        // so TenantScopedRepository sees CurrentTenantId == null and query filters are bypassed.
        services.AddScoped<ITenantContext, SystemTenantContext>();

        services.AddFacebookProvider(configuration);
        services.AddInstagramProvider();
        services.AddTikTokProvider();
        services.AddYouTubeProvider();

        services.AddProviderRegistries();
        services.AddServiceBusPublishJobSender(configuration);

        return services;
    }
}