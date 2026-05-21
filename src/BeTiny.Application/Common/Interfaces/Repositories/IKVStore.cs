namespace BeTiny.Application.Common.Interfaces.Repositories;

public interface IKVStore
{
    /// <summary>
    /// Gets the next hash seed for generating short URLs.
    /// </summary>
    /// <param name="ct">
    /// The cancellation token. Note: cancellation is checked before the 
    /// operation but the Redis operation itself is not cancellable.
    /// </param>
    /// <returns>The next hash seed</returns>
    Task<long> GetNextHashSeed(CancellationToken ct = default);
}
