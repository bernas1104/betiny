using System.Diagnostics.CodeAnalysis;
using BeTiny.IOC.DependencyInjections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC;

[ExcludeFromCodeCoverage]
public static class ConfigureDependencyInjection
{
  /// <summary>
  /// Registers all application dependencies including databases, handlers, services, validators, and options.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="configuration">The application configuration.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection RegisterBindings(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.RegisterOptions(configuration);
    services.RegisterDatabases(configuration);
    services.RegisterHandlers();
    services.RegisterServices();
    services.RegisterValidators();
    
    return services;
  }
}