using System.Linq.Expressions;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Events.ClickEvents.Create;
using BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;

namespace BeTiny.Tests.Application.Features.UrlShortening.Queries;

public class GetByShortCodeQueryTest
{
    private readonly Mock<IRepository<ShortUrl, ShortUrlId, Guid>> _shortUrlRepositoryMock;
    private readonly Mock<IIpResolver> _ipResolverMock;
    private readonly Mock<IDeviceDetector> _deviceDetectorMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<GetByShortCodeQuery>> _loggerMock = new ();
    private readonly GetByShortCodeQuery _query;
    private readonly Faker _faker = new ();

    public GetByShortCodeQueryTest()
    {
        _shortUrlRepositoryMock = new Mock<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _ipResolverMock = new Mock<IIpResolver>();
        _deviceDetectorMock = new Mock<IDeviceDetector>();
        _publisherMock = new Mock<IPublisher>();

        _query = new GetByShortCodeQuery(
            _shortUrlRepositoryMock.Object,
            _ipResolverMock.Object,
            _deviceDetectorMock.Object,
            _publisherMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenShortCodeExists()
    {
        // Arrange
        var expectedCountry = _faker.Address.Country();

        var shortUrl = new ShortUrl("http://example.com", "abc123");
        Expression<Func<ShortUrl, bool>> capturedExpression = null!;

        _shortUrlRepositoryMock.Setup(
            repo => repo.GetByFilterAsync(
                It.IsAny<Expression<Func<ShortUrl, bool>>>(),
                It.IsAny<CancellationToken>()
            )
        )
        .Callback<Expression<Func<ShortUrl, bool>>, CancellationToken>(
            (expr, _) => capturedExpression = expr
        )
        .ReturnsAsync(shortUrl);

        _ipResolverMock.Setup(
            resolver => resolver.GetCountryByIpAsync(
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(expectedCountry);

        _deviceDetectorMock.Setup(
            detector => detector.DetectDeviceType(
                It.IsAny<string?>()
            )
        ).Returns(DeviceTypes.Desktop);

        var request = new GetByShortCodeRequest(
            "abc123",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            "http://unittest.com",
            "127.0.0.1"
        );

        // Act
        var result = await _query.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("http://example.com", result.Value.OriginalUrl);
        Assert.Null(result.Value.ExpiresAt);

        _publisherMock.Verify(
            publisher => publisher.Publish(
                It.Is<CreateClickEventNotification>(n =>
                    n.ClickEvent.ShortUrlId == shortUrl.Id &&
                    n.ClickEvent.IpAddress == "127.0.0.1" &&
                    n.ClickEvent.UserAgent == "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" &&
                    n.ClickEvent.Referer == "http://unittest.com" &&
                    n.ClickEvent.DeviceType == DeviceTypes.Desktop &&
                    n.ClickEvent.Country == expectedCountry
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenShortCodeDoesNotExist()
    {
        // Arrange
        var nonExistentShortUrl = new ShortUrl("http://example.com", "nonexistent");

        _shortUrlRepositoryMock.Setup(
            repo => repo.GetByFilterAsync(
                It.Is<Expression<Func<ShortUrl, bool>>>(expr => 
                    expr.Compile()(nonExistentShortUrl) == true),
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync((ShortUrl?)null);

        var request = new GetByShortCodeRequest(
            "nonexistent",
            "UnitTestAgent",
            "http://unittest.com",
            "127.0.0.1"
        );

        // Act
        var result = await _query.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal("Short code not found.", result.Errors.First().ErrorMessage);

        _shortUrlRepositoryMock.Verify(
            repo => repo.GetByFilterAsync(
                It.Is<Expression<Func<ShortUrl, bool>>>(expr => 
                    expr.Compile()(nonExistentShortUrl) == true),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _publisherMock.Verify(
            publisher => publisher.Publish(
                It.IsAny<CreateClickEventNotification>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }
}
