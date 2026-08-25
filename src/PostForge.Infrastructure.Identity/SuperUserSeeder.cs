using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        await identityDbContext.Database.EnsureCreatedAsync();

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