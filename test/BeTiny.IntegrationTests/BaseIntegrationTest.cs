using BeTiny.IntegrationTests.Fixtures;

namespace BeTiny.IntegrationTests;

[Collection("IntegrationTests")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestFixture Fixture;
    protected readonly ApiWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
        Factory = fixture.Factory;
        Client = Factory.CreateClient();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
    }
}
