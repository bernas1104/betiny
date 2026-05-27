using BeTiny.Domain.Enums;

namespace BeTiny.Application.Common.Helpers;

public static class DeviceTypeHelper
{
    public static DeviceTypes DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return DeviceTypes.Unknown;

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("tablet") || ua.Contains("ipad") || ua.Contains("playbook") || ua.Contains("silk"))
            return DeviceTypes.Tablet;

        if (ua.Contains("android") && !ua.Contains("mobile"))
            return DeviceTypes.Tablet;

        if (
            ua.Contains("mobile") || ua.Contains("iphone") || ua.Contains("ipod") ||
            ua.Contains("android") || ua.Contains("blackberry") || ua.Contains("windows phone")
        )
            return DeviceTypes.Mobile;

        if (
            ua.Contains("mozilla") || ua.Contains("chrome") || ua.Contains("safari") ||
            ua.Contains("opera") || ua.Contains("msie") || ua.Contains("trident") ||
            ua.Contains("edge") || ua.Contains("firefox")
        )
            return DeviceTypes.Desktop;

        return DeviceTypes.Unknown;
    }
}
