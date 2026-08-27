using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace PostForge.Infrastructure.Identity;

public static class SuperUserSeeder
{
    public static async Task EnsureIdentityDatabaseAndSuperUserAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var identityDbContext = sp.GetRequiredService<AppIdentityDbContext>();
        // Use Migrate if migrations exist (preferred), fallback to EnsureCreated for legacy DBs.
        // Both contexts share the same DB, so we must not assume the DB is empty.
        var pending = await identityDbContext.Database.GetPendingMigrationsAsync();
        if (pending.Any())
        {
            try
            {
                await identityDbContext.Database.MigrateAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                // 42P07 = relation already exists (DB created via EnsureCreated before migrations)
                var env = sp.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    // In Development, safe to recreate — but we must not drop business tables.
                    // Instead, baseline by inserting history row if tables already exist.
                    // Simplest dev fix: ensure created fallback
                    await identityDbContext.Database.EnsureCreatedAsync();
                }
                else throw;
            }
        }
        else
        {
            var applied = await identityDbContext.Database.GetAppliedMigrationsAsync();
            if (!applied.Any())
            {
                // No migrations history at all (fresh DB without any migrations) -> EnsureCreated
                // But we already have business migrations, so history exists -> this branch not taken
                // Keep EnsureCreated as fallback for environments without migrations
                await identityDbContext.Database.EnsureCreatedAsync();
            }
        }

        var superUser = configuration.GetSection("Auth:SuperUser").Get<SuperUserOptions>();
        var email = superUser?.Email;
        var password = superUser?.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("SuperUserSeeder");
            logger?.LogWarning(
                "Auth:SuperUser:Email or Auth:SuperUser:Password not configured — skipping super user seeding. "
                + "Set via environment variables (Auth__SuperUser__Email / Auth__SuperUser__Password), user-secrets, or Key Vault.");
            return;
        }

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            IsSuperUser = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to seed super user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}