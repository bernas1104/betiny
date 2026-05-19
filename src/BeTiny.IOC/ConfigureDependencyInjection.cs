using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Features.Services;
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
    services.RegisterHandlers();
    services.RegisterServices();
    services.RegisterValidators();
    
    return services;
  }
}