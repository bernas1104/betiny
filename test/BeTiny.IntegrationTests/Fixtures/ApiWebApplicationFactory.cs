using BeTiny.IntegrationTests.Fixtures;
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
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public new Task DisposeAsync() => Task.CompletedTask;
}
