using BeTiny.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Resolves country from IP address. Currently a placeholder that always returns "Unknown".
/// </summary>
public sealed class IpResolver : IIpResolver
{
    private readonly ILogger<IpResolver> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpResolver"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public IpResolver(ILogger<IpResolver> logger)
    {
        _logger = logger;
    }

    // TODO: Integrate with a real IP geolocation service (e.g., MaxMind, IP2Location)
    /// <inheritdoc/>
    public Task<string> GetCountryByIpAsync(
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        _logger.LogWarning("IP geolocation not configured. Returning 'Unknown'");

        // Placeholder implementation - always returns "Unknown"
        return Task.FromResult("Unknown");
    }
}
