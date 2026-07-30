using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostForge.Api.Controllers;
using PostForge.Infrastructure.Persistence;

namespace PostForge.IntegrationTests.Api;

public class PostForgeWebApplicationFactory : WebApplicationFactory<PostsController>, IAsyncLifetime
{
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
                ["ConnectionStrings:ServiceBus"] = "Endpoint=sb://placeholder.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test"
            });
        });
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        await using var context = new PostForgeDbContext(options);
        await context.Database.EnsureCreatedAsync();
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
