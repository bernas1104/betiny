using BeTiny.Infrastructure.Postgres.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections
{
    public static class ConfigureDatabases
    {
        public static IServiceCollection RegisterDatabases(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var postgresConnString = configuration.GetConnectionString("Postgres");
            if (string.IsNullOrEmpty(postgresConnString))
                throw new ArgumentException("Postgres connection string must be provided.");

            services.AddDbContext<BeTinyContext>(options => 
                options.UseNpgsql(postgresConnString)
            );

            return services;
        }
    }
}