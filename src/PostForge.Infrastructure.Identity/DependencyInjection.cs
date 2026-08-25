using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PostForge.Application.Common.Interfaces;
using PostForge.Domain.Interfaces;
using PostForge.Infrastructure.Identity.Tenancy;

namespace PostForge.Infrastructure.Identity;

public static class DependencyInjection
{
    public const string SuperAdminPolicy = "SuperAdmin";

    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityCoreInfrastructure(configuration);

        var authOptions = configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SuperAdminPolicy, policy =>
                policy.RequireClaim("isSuperUser", "true"));
        });

        return services;
    }

    public static IServiceCollection AddIdentityCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostForgeDb")
                ?? throw new InvalidOperationException("Connection string 'PostForgeDb' not found.")));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ApplicationTenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ApplicationTenantContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}