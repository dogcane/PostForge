using ECO.Data;
using ECO.Providers.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.Infrastructure.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPersistenceUnitFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("PostForgeDb")
                ?? throw new InvalidOperationException("Connection string 'PostForgeDb' not found.");

            var dbContextOptions = new DbContextOptionsBuilder<PostForgeDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var factory = new PersistenceUnitFactory();
            var unit = new EntityFrameworkPersistenceUnit<PostForgeDbContext>(
                "PostForgeUnit",
                dbContextOptions,
                null);

            unit.AddClass<Post, Guid>();
            unit.AddClass<Campaign, Guid>();
            unit.AddClass<ScheduleSlot, Guid>();
            unit.AddClass<SocialAccount, Guid>();
            unit.AddClass<ProviderCredential, Guid>();

            factory.AddPersistenceUnit(unit);
            return factory;
        });

        services.AddScoped<IDataContext>(sp =>
            sp.GetRequiredService<IPersistenceUnitFactory>().OpenDataContext());

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IScheduleSlotRepository, ScheduleSlotRepository>();
        services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
        services.AddScoped<IProviderCredentialRepository, ProviderCredentialRepository>();

        return services;
    }
}
