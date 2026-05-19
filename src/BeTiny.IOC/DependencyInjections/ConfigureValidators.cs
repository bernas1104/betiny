using BeTiny.Application;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

public static class ConfigureValidators
{
    public static IServiceCollection RegisterValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

        return services;
    }
}
