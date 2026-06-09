using BeTiny.Domain.Enums;

namespace BeTiny.Application.Common.Interfaces.Services;

/// <summary>
/// Detects the device type from a user agent string.
/// </summary>
public interface IDeviceDetector
{
    /// <summary>
    /// Detects the device type from the provided user agent string.
    /// </summary>
    /// <param name="userAgent">The user agent string. Can be null.</param>
    /// <returns>The detected device type.</returns>
    DeviceTypes DetectDeviceType(string? userAgent);
}
