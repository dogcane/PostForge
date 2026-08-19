using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace PostForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}