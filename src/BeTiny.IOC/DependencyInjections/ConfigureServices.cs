using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.Services;
using BeTiny.Domain.Interfaces;
using BeTiny.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

public static class ConfigureServices
{
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
