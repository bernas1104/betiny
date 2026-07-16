using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BeTiny.IntegrationTests.Fixtures;

public class InfrastructureFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("betiny_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public RedisContainer Redis { get; } = new RedisBuilder("redis:7-alpine")
        .Build();

    public string PostgresConnectionString => Postgres.GetConnectionString();
    public string RedisConnectionString => Redis.GetConnectionString();
    public string JwtIssuer { get; } = "https://betiny.io/";
    public string JwtAudience { get; } = "https://betiny.io/";
    public string JwtSigningKey { get; } = "xOkTe4bXXv9UHjgGEAbCRahs5Icbrjw7s1089l4W9gI=";
    public int ExpiryMinutes { get; } = 60;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(Postgres.StartAsync(), Redis.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(Postgres.DisposeAsync().AsTask(), Redis.DisposeAsync().AsTask());
    }
}
