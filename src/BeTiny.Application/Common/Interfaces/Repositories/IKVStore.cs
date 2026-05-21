namespace BeTiny.Application.Common.Interfaces.Repositories;

public interface IKVStore
{
    /// <summary>
    /// Gets the next hash seed for generating short URLs.
    /// </summary>
    /// <returns>The next hash seed</returns>
    Task<long> GetNextHashSeed();
}
