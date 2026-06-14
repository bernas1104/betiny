using BeTiny.Infrastructure.Redis;
using StackExchange.Redis;

namespace BeTiny.UnitTests.Infrastructure.Redis;

public sealed class KVStoreTest
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly KVStore _kvStore;

    public KVStoreTest()
    {
        _connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _connectionMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(_database);
        _kvStore = new KVStore(_connectionMultiplexer);
    }

    [Fact]
    public async Task GetNextHashSeed_WhenCalled_ShouldReturnIncrementedValue()
    {
        _database.StringIncrementAsync("UrlShortener:Counter")
            .Returns(Task.FromResult(5L));

        var result = await _kvStore.GetNextHashSeed();

        result.Should().Be(4L);
    }
}
