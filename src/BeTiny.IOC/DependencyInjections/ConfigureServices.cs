using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Options;
using BeTiny.Application.Features.Services;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Interfaces;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace BeTiny.IOC.DependencyInjections;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    /// <summary>
    /// Registers application and infrastructure services for dependency injection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection RegisterServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddRefitClient<IIpApi>()
            .ConfigureHttpClient(c =>
            {
                var ipApiOptions = configuration.GetSection("IpApi").Get<IpApiOptions>();
                if (ipApiOptions == null)
                    throw new InvalidOperationException("IpApi options are not configured.");
                
                if (string.IsNullOrEmpty(ipApiOptions.BaseUrl))
                    throw new InvalidOperationException("IpApi BaseUrl is not configured.");

                if (ipApiOptions.TimeoutSeconds <= 0)
                    throw new InvalidOperationException("IpApi TimeoutSeconds must be greater than zero.");

                c.BaseAddress = new Uri(ipApiOptions.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(ipApiOptions.TimeoutSeconds);
            });
        
        services.AddSingleton<ReservedAliasDefaults>();

        services.AddScoped<IShortCodeGenerator, ShortCodeGenerator>();
        services.AddScoped<IIpResolver, IpResolver>();
        services.AddScoped<IDeviceDetector, DeviceDetector>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IReservedAliasPolicy, ReservedAliasPolicy>();
        
        return services;
    }
}
