using BeTiny.Api.Domain.Interfaces;

namespace BeTiny.Api.Application
{
    public static class ApplicationConfiguration
    {
        public static IServiceCollection ConfigureApplication(
            this IServiceCollection services
        )
        {
            services.Scan(
                scan => scan.FromAssembliesOf(typeof(Program))
                    .AddClasses(
                        classes => classes.AssignableTo(typeof(IQueryHandler<,>)),
                        publicOnly: false
                    )
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(
                        classes => classes.AssignableTo(typeof(ICommandHandler<,>)),
                        publicOnly: false
                    )
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }
    }
}
