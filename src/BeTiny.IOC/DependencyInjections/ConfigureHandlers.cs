using BeTiny.Application;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

public static class ConfigureHandlers
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        services.Scan(
            scan => scan.FromAssembliesOf(typeof(AssemblyReference))
                .AddClasses(
                    classes => classes.AssignableTo(typeof(IRequestHandler<,>)),
                    publicOnly: false
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(
                    classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)),
                    publicOnly: false
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(
                    classes => classes.AssignableTo(typeof(ISender)),
                    publicOnly: false
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(
                    classes => classes.AssignableTo(typeof(INotificationHandler<>)),
                    publicOnly: false
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(
                    classes => classes.AssignableTo(typeof(IPublisher)),
                    publicOnly: false
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
