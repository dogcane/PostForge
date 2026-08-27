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
        // Register PostForgeDbContext directly so it can be resolved for EnsureCreated/Migrate
        // and for design-time tools (dotnet ef). The ECO factory reuses the same connection string.
        services.AddDbContext<PostForgeDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("PostForgeDb")
                ?? throw new InvalidOperationException("Connection string 'PostForgeDb' not found.");
            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IPersistenceUnitFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("PostForgeDb")
                ?? throw new InvalidOperationException("Connection string 'PostForgeDb' not found.");

            var dbContextOptions = new DbContextOptionsBuilder<PostForgeDbContext>()
                .UseNpgsql(connectionString)
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
            unit.AddClass<Tenant, Guid>();
            unit.AddClass<TenantMembership, Guid>();
            // MediaAsset is not an AggregateRoot (it's Entity owned via Post._mediaAssetsField)
            // so it is NOT registered with ECO's AddClass, but it IS mapped in PostForgeDbContext via DbSet+ToTable.

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
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantMembershipRepository, TenantMembershipRepository>();

        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations for <see cref="PostForgeDbContext"/>.
    /// Uses <see cref="RelationalDatabaseFacadeExtensions.MigrateAsync"/> as the single
    /// source of truth for schema creation/evolution. Handles both fresh DBs
    /// (creates history table + all migrations) and existing DBs (applies only pending).
    /// Call at startup (Api + Worker) before first query.
    /// </summary>
    public static async Task EnsurePostForgeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostForgeDbContext>();
        await context.Database.MigrateAsync();
    }
}