using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.Interfaces.Repositories;
using BeTiny.Api.Domain.ValueObjects;
using BeTiny.Api.Infra.Database.Context;
using BeTiny.Api.Infra.Database.Repositories;
using BeTiny.Api.Infra.KVStore;

using Microsoft.EntityFrameworkCore;

using StackExchange.Redis;

namespace BeTiny.Api.Infra
{
    public static class InfraConfiguration
    {
        public static IServiceCollection ConfigureInfra(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var ngpsqlConnString = configuration.GetConnectionString("Npgsql");
            if (string.IsNullOrEmpty(ngpsqlConnString))
            {
                throw new ArgumentException("NpgSQL connection string must be provided.");
            }

            var redisConnString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnString))
            {
                throw new ArgumentException("Redis connection string must be provided.");
            }

            services.AddDbContext<BeTinyDbContext>(
                opt => opt.UseNpgsql(ngpsqlConnString)
            );

            // services.AddNpgsql<BeTinyDbContext>(ngpsqlConnString);

            services.AddSingleton<IConnectionMultiplexer>(
                sp => ConnectionMultiplexer.Connect(redisConnString)
            );

            services.AddScoped<IKVStore, RedisStore>();

            services.AddHealthChecks()
                .AddNpgSql(ngpsqlConnString, name: "[NpgSQL]")
                .AddRedis(redisConnString, name: "[Redis]");

            services.AddScoped<
                IRepository<Url, UrlId, string>,
                GenericRepository<Url, UrlId, string>
            >();

            return services;
        }
    }
}
