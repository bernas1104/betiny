using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

[ExcludeFromCodeCoverage]
public static class ConfigureOptions
{
    /// <summary>
    /// Registers options configuration from app settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection RegisterOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Place to register options configuration from app settings, e.g.:
        // services.Configure<MyOptions>(configuration.GetSection("MyOptions"));

        return services;
    }
}
