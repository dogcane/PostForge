using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PostForge.Infrastructure.DAL;

/// <summary>
/// Design-time factory for `dotnet ef migrations` commands.
/// Uses the same connection string key as runtime (ConnectionStrings:PostForgeDb).
/// </summary>
public class DesignTimePostForgeDbContextFactory : IDesignTimeDbContextFactory<PostForgeDbContext>
{
    public PostForgeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PostForge.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PostForgeDb")
            ?? "Host=localhost;Port=5432;Database=PostForgeDb_Dev;Username=postgres;Password=postforge";

        var optionsBuilder = new DbContextOptionsBuilder<PostForgeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PostForgeDbContext(optionsBuilder.Options);
    }
}
