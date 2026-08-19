using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Api.Controllers;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.Identity;

namespace PostForge.IntegrationTests.Api;

public class PostForgeWebApplicationFactory : WebApplicationFactory<PostsController>, IAsyncLifetime
{
    public const string SuperUserEmail = "admin@postforge.test";
    public const string SuperUserPassword = "Admin!12345";

    private readonly string _connectionString;
    private bool _disposed;

    public PostForgeWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostForgeDb"] = _connectionString,
                ["ConnectionStrings:ServiceBus"] = "Endpoint=sb://placeholder.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
                ["Auth:Issuer"] = "PostForge",
                ["Auth:Audience"] = "PostForge.Api",
                ["Auth:SecretKey"] = "test-secret-key-with-at-least-32-characters!!",
                ["Auth:ExpiresInMinutes"] = "60",
                ["Auth:SuperUser:Email"] = SuperUserEmail,
                ["Auth:SuperUser:Password"] = SuperUserPassword
            });
        });
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var context = new PostForgeDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task SeedIdentityAsync()
    {
        var identityOptions = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var identityContext = new AppIdentityDbContext(identityOptions);
        await identityContext.Database.EnsureCreatedAsync();

        var userManager = Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(SuperUserEmail) is null)
        {
            var result = await userManager.CreateAsync(new ApplicationUser
            {
                UserName = SuperUserEmail,
                Email = SuperUserEmail,
                IsSuperUser = true
            }, SuperUserPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to seed test super user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }

    public new async Task DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            await base.DisposeAsync();
        }
    }
}
