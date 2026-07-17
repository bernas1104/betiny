namespace BeTiny.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    public InfrastructureFixture Infrastructure { get; }
    public ApiWebApplicationFactory Factory { get; }
    public TestTokenFactory TokenFactory { get; }

    public IntegrationTestFixture(InfrastructureFixture infrastructure)
    {
        Infrastructure = infrastructure;
        Factory = new ApiWebApplicationFactory(infrastructure);
        TokenFactory = new TestTokenFactory(infrastructure);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}
