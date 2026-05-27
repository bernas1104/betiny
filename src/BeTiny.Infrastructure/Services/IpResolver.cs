using BeTiny.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BeTiny.Infrastructure.Services;

public sealed class IpResolver : IIpResolver
{
    private readonly ILogger<IpResolver> _logger;

    public IpResolver(ILogger<IpResolver> logger)
    {
        _logger = logger;
    }

    // TODO: Integrate with a real IP geolocation service (e.g., MaxMind, IP2Location)
    public Task<string> GetCountryByIpAsync(
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        _logger.LogWarning("IP geolocation not configured. Returning 'Unknown' for IP: {IpAddress}", ipAddress ?? "null");

        // Placeholder implementation - always returns "Unknown"
        return Task.FromResult("Unknown");
    }
}
