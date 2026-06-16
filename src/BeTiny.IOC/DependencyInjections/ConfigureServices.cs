using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.Services;
using BeTiny.Domain.Interfaces;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

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
        this IServiceCollection services
    )
    {
        services.AddScoped<IShortCodeGenerator, ShortCodeGenerator>();
        services.AddScoped<IIpResolver, IpResolver>();
        services.AddScoped<IDeviceDetector, DeviceDetector>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        
        return services;
    }
}
