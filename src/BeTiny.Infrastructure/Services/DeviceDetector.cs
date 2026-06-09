using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Domain.Enums;
using UAParser;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Service for detecting the type of device based on the user agent string.
/// </summary>
public sealed class DeviceDetector : IDeviceDetector
{
    private readonly Parser _parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceDetector"/> class.
    /// </summary>
    public DeviceDetector()
    {
        _parser = Parser.GetDefault();
    }

    
    /// <summary>
    /// Detects the type of device based on the user agent string.
    /// </summary>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>The detected device type.</returns>
    public DeviceTypes DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return DeviceTypes.Unknown;

        var clientInfo = _parser.Parse(userAgent);
        var deviceFamily = clientInfo.Device
            .Family?
            .ToLowerInvariant() ?? string.Empty;
        var uaFamily = clientInfo.UA
            .Family?
            .ToLowerInvariant() ?? string.Empty;

        var uaLower = userAgent.ToLowerInvariant();

        if (IsBot(deviceFamily))
            return DeviceTypes.Unknown;

        if (IsTablet(deviceFamily, uaLower, uaFamily))
            return DeviceTypes.Tablet;

        if (IsMobile(deviceFamily, uaLower, uaFamily))
            return DeviceTypes.Mobile;

        if (IsUnknownBrowser(uaFamily))
            return DeviceTypes.Unknown;

        return DeviceTypes.Desktop;
    }

    private static bool IsTablet(
        string deviceFamily,
        string userAgent,
        string uaFamily
    )
    {
        if (deviceFamily.Contains("ipad") || deviceFamily.Contains("playbook"))
            return true;

        if (deviceFamily.Contains("silk") || deviceFamily.Contains("kindle fire"))
            return true;

        if (userAgent.Contains("android") && !userAgent.Contains("mobile"))
            return true;

        return false;
    }

    private static bool IsMobile(
        string deviceFamily,
        string userAgent,
        string uaFamily
    )
    {
        if (deviceFamily.Contains("iphone") || deviceFamily.Contains("ipod"))
            return true;

        if (deviceFamily.Contains("windows phone") || deviceFamily.Contains("blackberry"))
            return true;

        if (uaFamily.Contains("mobile") || userAgent.Contains("mobile"))
            return true;

        return false;
    }

    private static bool IsBot(string deviceFamily)
    {
        return deviceFamily.Contains("spider")
            || deviceFamily.Contains("bot")
            || deviceFamily.Contains("crawler");
    }

    private static bool IsUnknownBrowser(string uaFamily)
    {
        return uaFamily == "other"
            || uaFamily == string.Empty
            || uaFamily.Contains("curl")
            || uaFamily.Contains("wget")
            || uaFamily.Contains("httpclient");
    }
}
