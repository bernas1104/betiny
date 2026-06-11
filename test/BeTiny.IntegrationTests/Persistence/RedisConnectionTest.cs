using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IntegrationTests.Persistence;

[Collection("IntegrationTests")]
public class RedisConnectionTest : BaseIntegrationTest, IClassFixture<IntegrationTestFixture>
{
    public RedisConnectionTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CanIncrementCounter()
    {
        using var scope = Factory.Services.CreateScope();
        var kvStore = scope.ServiceProvider.GetRequiredService<IKVStore>();

        var seed1 = await kvStore.GetNextHashSeed();
        var seed2 = await kvStore.GetNextHashSeed();

        seed2.Should().Be(seed1 + 1);
    }
}
