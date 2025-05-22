namespace BeTiny.Api.Domain.Interfaces.Repositories
{
    public interface IKVStore
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<long> GetNextHashSeed(CancellationToken cancellationToken);
        Task<bool> HasKeyAsync(string key, CancellationToken cancellationToken = default);
        Task SetAsync(string key, object? value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    }
}
