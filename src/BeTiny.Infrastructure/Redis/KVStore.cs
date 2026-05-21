using BeTiny.Application.Common.Interfaces.Repositories;
using StackExchange.Redis;

namespace BeTiny.Infrastructure.Redis;

public class KVStore : IKVStore
{
    private readonly IDatabase _database;

    public KVStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<long> GetNextHashSeed()
    {
        return (await _database.StringIncrementAsync("UrlShortener:Counter")) - 1;
    }
}
