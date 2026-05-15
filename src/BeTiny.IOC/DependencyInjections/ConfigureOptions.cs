using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

public static class ConfigureOptions
{
    public static IServiceCollection RegisterOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //

        return services;
    }
}
