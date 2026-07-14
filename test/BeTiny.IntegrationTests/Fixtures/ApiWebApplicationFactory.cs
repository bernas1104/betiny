using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeTiny.IntegrationTests.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly InfrastructureFixture _infrastructure;

    public ApiWebApplicationFactory(InfrastructureFixture infrastructure)
    {
        _infrastructure = infrastructure;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting("ConnectionStrings:Postgres", _infrastructure.PostgresConnectionString);
        builder.UseSetting("ConnectionStrings:Redis", _infrastructure.RedisConnectionString);
        builder.UseSetting("Jwt:Issuer", _infrastructure.JwtIssuer);
        builder.UseSetting("Jwt:Audience", _infrastructure.JwtAudience);
        builder.UseSetting("Jwt:SigningKey", _infrastructure.JwtSigningKey);
        builder.UseSetting("Jwt:ExpiryMinutes", _infrastructure.ExpiryMinutes.TotalMinutes.ToString());
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
