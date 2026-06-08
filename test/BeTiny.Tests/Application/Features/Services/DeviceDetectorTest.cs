using BeTiny.Domain.Enums;
using BeTiny.Infrastructure.Services;

namespace BeTiny.Tests.Application.Features.Services;

public class DeviceDetectorTest
{
    private readonly DeviceDetector _deviceDetector = new();

    [Fact]
    public void DetectDeviceType_NullUserAgent_ReturnsUnknown()
    {
        var result = _deviceDetector.DetectDeviceType(null);

        Assert.Equal(DeviceTypes.Unknown, result);
    }

    [Fact]
    public void DetectDeviceType_EmptyUserAgent_ReturnsUnknown()
    {
        var result = _deviceDetector.DetectDeviceType(string.Empty);

        Assert.Equal(DeviceTypes.Unknown, result);
    }

    [Fact]
    public void DetectDeviceType_WhitespaceUserAgent_ReturnsUnknown()
    {
        var result = _deviceDetector.DetectDeviceType("   ");

        Assert.Equal(DeviceTypes.Unknown, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36", DeviceTypes.Desktop)]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64; rv:120.0) Gecko/20100101 Firefox/120.0", DeviceTypes.Desktop)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 14_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15", DeviceTypes.Desktop)]
    public void DetectDeviceType_DesktopBrowsers_ReturnsDesktop(string userAgent, DeviceTypes expected)
    {
        var result = _deviceDetector.DetectDeviceType(userAgent);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", DeviceTypes.Mobile)]
    [InlineData("Mozilla/5.0 (Linux; Android 14; SM-S911B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36", DeviceTypes.Mobile)]
    public void DetectDeviceType_MobileBrowsers_ReturnsMobile(string userAgent, DeviceTypes expected)
    {
        var result = _deviceDetector.DetectDeviceType(userAgent);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", DeviceTypes.Tablet)]
    [InlineData("Mozilla/5.0 (Linux; Android 13; SM-X900) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36", DeviceTypes.Tablet)]
    public void DetectDeviceType_TabletBrowsers_ReturnsTablet(string userAgent, DeviceTypes expected)
    {
        var result = _deviceDetector.DetectDeviceType(userAgent);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", DeviceTypes.Unknown)]
    [InlineData("curl/7.88.1", DeviceTypes.Unknown)]
    public void DetectDeviceType_BotsAndTools_ReturnsUnknown(string userAgent, DeviceTypes expected)
    {
        var result = _deviceDetector.DetectDeviceType(userAgent);

        Assert.Equal(expected, result);
    }
}
