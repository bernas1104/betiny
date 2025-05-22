namespace BeTiny.Api.Domain.Interfaces.Repositories
{
    public interface IKVStore
    {
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync(string key, object? value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    }
}
