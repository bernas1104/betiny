using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Infrastructure.Services;
using BeTiny.UnitTests.MotherObjects;
using Bogus;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Infrastructure.Services;

public sealed class IpResolverTest
{
    private readonly IIpApi _ipApi;
    private readonly ILogger<IpResolver> _logger;
    private readonly IpResolver _ipResolver;
    private readonly Faker _faker = new();

    public IpResolverTest()
    {
        _ipApi = Substitute.For<IIpApi>();
        _logger = Substitute.For<ILogger<IpResolver>>();
        _ipResolver = new (_ipApi, _logger);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetCountryByIpAsync_WhenIpAddressIsNullOrEmpty_ReturnsUnknown(string? ipAddress)
    {
        // Act
        var result = await _ipResolver.GetCountryByIpAsync(ipAddress);

        // Assert
        result.Should().Be("Unknown");

        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Information
                && call.GetArguments()[3] is null)
            .Should().ContainSingle();
    }

    [Fact]
    public async Task GetCountryByIpAsync_WhenIpAddressIsValid_ReturnsCountry()
    {
        // Arrange
        var ipAddress = _faker.Internet.IpAddress()
            .MapToIPv4()
            .ToString();

        var ipApiResponse = IpApiResponseMotherObject.GetResponse();
        var expectedCountry = ipApiResponse.Country;

        _ipApi.GetIpInfoAsync(ipAddress)
            .Returns(ipApiResponse);

        // Act
        var country = await _ipResolver.GetCountryByIpAsync(ipAddress);

        // Assert
        country.Should().NotBeNullOrEmpty();
        country.Should().Be(expectedCountry);

        await _ipApi.Received(1).GetIpInfoAsync(ipAddress);
    }

    [Fact]
    public async Task GetCountryByIpAsync_WhenIpApiThrowsException_ReturnsUnknown()
    {
        // Arrange
        var ipAddress = _faker.Internet.IpAddress()
            .MapToIPv4()
            .ToString();

        _ipApi.GetIpInfoAsync(ipAddress)
            .ThrowsAsync(new Exception("IP API error"));

        // Act
        var country = await _ipResolver.GetCountryByIpAsync(ipAddress);

        // Assert
        country.Should().Be("Unknown");

        await _ipApi.Received(1).GetIpInfoAsync(ipAddress);
        
        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning
                && call.GetArguments()[3] is not null)
            .Should().ContainSingle();
    }
}
