using BeTiny.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Resolves country from IP address using an external IP API.
/// </summary>
public sealed class IpResolver(IIpApi ipApi, ILogger<IpResolver> logger) : IIpResolver
{
    /// <inheritdoc/>
    public async Task<string> GetCountryByIpAsync(string? ipAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                logger.LogInformation("No IP address provided. Returning 'Unknown'.");
                return "Unknown";
            }

            var ipInfo = await ipApi.GetIpInfoAsync(ipAddress);
            
            if (!ipInfo.Success)
            {
                logger.LogInformation("Failed to resolve IP address. Returning 'Unknown'. Error: {Error}",
                    ipInfo.Message);

                return "Unknown";
            }
            
            return ipInfo.Country ?? "Unknown";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error occurred while resolving IP address. Returning 'Unknown'.");
            return "Unknown";
        }
    }
}
