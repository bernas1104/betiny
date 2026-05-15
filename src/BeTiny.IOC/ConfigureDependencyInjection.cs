using System.Diagnostics.CodeAnalysis;
using BeTiny.IOC.DependencyInjections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC;

[ExcludeFromCodeCoverage]
public static class ConfigureDependencyInjection
{
  public static IServiceCollection RegisterBindings(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.RegisterOptions(configuration);
    services.RegisterDatabases(configuration);
    
    return services;
  }
}
