namespace BeTiny.Application.Common.Interfaces.Services;

/// <summary>
/// Resolves the country based on the provided IP address.
/// </summary>
public interface IIpResolver
{
    /// <summary>
    /// Gets the country based on the provided IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to resolve. Can be null.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The country associated with the IP address.</returns>
    Task<string> GetCountryByIpAsync(
        string? ipAddress,
        CancellationToken ct = default
    );
}
