using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        var email = configuration["Auth:SuperUser:Email"] ?? "admin@postforge.dev";
        var password = configuration["Auth:SuperUser:Password"] ?? "Admin!12345";

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