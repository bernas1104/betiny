using System.Text.Json;

using BeTiny.Api.Domain.Interfaces.Repositories;

using StackExchange.Redis;

namespace BeTiny.Api.Infra.KVStore
{
    public class RedisStore : IKVStore
    {
        private readonly IDatabase _database;

        public RedisStore(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stringValue = await _database.StringGetAsync(key);
            if (stringValue.IsNull)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                stringValue!,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        public async Task<long> GetNextHashSeed(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hashSeed = (await _database.StringIncrementAsync("Counter")) - 1;

            return hashSeed;
        }

        public async Task SetAsync(string key, object? value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stringValue = JsonSerializer.Serialize(
                value,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );

            await _database.StringSetAsync(key, stringValue);

            if (expiry is not null)
            {
                await _database.KeyExpireAsync(key, expiry);
            }
        }
    }
}
