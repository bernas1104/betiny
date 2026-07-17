using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BeTiny.IntegrationTests.Fixtures;

public class InfrastructureFixture : IAsyncLifetime
{
    private readonly IConfiguration _configuration;

    public InfrastructureFixture()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json")
            .Build();
    }

    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("betiny_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public RedisContainer Redis { get; } = new RedisBuilder("redis:7-alpine")
        .Build();

    public string PostgresConnectionString => Postgres.GetConnectionString();
    public string RedisConnectionString => Redis.GetConnectionString();
    public string JwtIssuer => _configuration["Jwt:Issuer"]!;
    public string JwtAudience => _configuration["Jwt:Audience"]!;
    public string JwtSigningKey => _configuration["Jwt:SigningKey"]!;
    public int JwtExpiryMinutes => int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

    public async Task InitializeAsync()
    {
        await Task.WhenAll(Postgres.StartAsync(), Redis.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(Postgres.DisposeAsync().AsTask(), Redis.DisposeAsync().AsTask());
    }
}
