using BeTiny.Api.Domain.Interfaces;

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

        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, object? value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
