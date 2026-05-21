using System.Diagnostics.CodeAnalysis;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.Infrastructure.Postgres.Repositories;
using BeTiny.Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BeTiny.IOC.DependencyInjections;

[ExcludeFromCodeCoverage]
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

        var redisConnString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnString))
            throw new ArgumentException("Redis connection string must be provided.");

        services.AddDbContext<BeTinyContext>(options => 
            options.UseNpgsql(postgresConnString)
        );

        services.AddSingleton<IConnectionMultiplexer>(
            sp => ConnectionMultiplexer.Connect(redisConnString)
        );

        services.AddScoped<IKVStore, KVStore>();

        services.AddScoped(typeof(IRepository<,,>), typeof(GenericRepository<,,>));

        return services;
    }
}
