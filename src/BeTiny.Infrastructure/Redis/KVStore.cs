using BeTiny.Application.Common.Interfaces.Repositories;
using StackExchange.Redis;

namespace BeTiny.Infrastructure.Redis;

/// <summary>
/// Redis-backed key-value store implementing <see cref="IKVStore"/>.
/// </summary>
public class KVStore : IKVStore
{
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="KVStore"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    public KVStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    /// <inheritdoc/>
    public async Task<long> GetNextHashSeed(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return (await _database.StringIncrementAsync("UrlShortener:Counter")) - 1;
    }
}
