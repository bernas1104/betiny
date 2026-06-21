using BeTiny.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Resolves country from IP address using an external IP API.
/// </summary>
public sealed class IpResolver : IIpResolver
{
    private readonly IIpApi _ipApi;
    private readonly ILogger<IpResolver> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpResolver"/> class.
    /// </summary>
    /// <param name="ipApi">The IP API instance.</param>
    /// <param name="logger">The logger instance.</param>
    public IpResolver(IIpApi ipApi, ILogger<IpResolver> logger)
    {
        _ipApi = ipApi;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetCountryByIpAsync(
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        try
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogInformation("No IP address provided. Returning 'Unknown'.");
                return "Unknown";
            }

            var ipInfo = await _ipApi.GetIpInfoAsync(ipAddress, ct);
            return ipInfo.Country ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred while resolving IP address. Returning 'Unknown'.");
            return "Unknown";
        }
    }
}
