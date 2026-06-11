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

namespace BeTiny.UnitTests.Application.Features.UrlShortening.Queries;

public class GetByShortCodeQueryTest
{
    private readonly IRepository<ShortUrl, ShortUrlId, Guid> _shortUrlRepository;
    private readonly IIpResolver _ipResolver;
    private readonly IDeviceDetector _deviceDetector;
    private readonly IPublisher _publisher;
    private readonly ILogger<GetByShortCodeQuery> _logger = Substitute.For<ILogger<GetByShortCodeQuery>>();
    private readonly GetByShortCodeQuery _query;
    private readonly Faker _faker = new();

    public GetByShortCodeQueryTest()
    {
        _shortUrlRepository = Substitute.For<IRepository<ShortUrl, ShortUrlId, Guid>>();
        _ipResolver = Substitute.For<IIpResolver>();
        _deviceDetector = Substitute.For<IDeviceDetector>();
        _publisher = Substitute.For<IPublisher>();

        _query = new GetByShortCodeQuery(
            _shortUrlRepository,
            _ipResolver,
            _deviceDetector,
            _publisher,
            _logger
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenShortCodeExists()
    {
        // Arrange
        var expectedCountry = _faker.Address.Country();

        var shortUrl = new ShortUrl("http://example.com", "abc123");
        Expression<Func<ShortUrl, bool>>? capturedExpression = null;

        _shortUrlRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<ShortUrl, bool>>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(shortUrl)
        .AndDoes(x => capturedExpression = x.ArgAt<Expression<Func<ShortUrl, bool>>>(0));

        _ipResolver.GetCountryByIpAsync(
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(expectedCountry);

        _deviceDetector.DetectDeviceType(
            Arg.Any<string?>()
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
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OriginalUrl.Should().Be("http://example.com");
        result.Value.ExpiresAt.Should().BeNull();

        await _publisher.Received(1).Publish(
            Arg.Is<CreateClickEventNotification>(n =>
                n.ClickEvent.ShortUrlId == shortUrl.Id &&
                n.ClickEvent.IpAddress == "127.0.0.1" &&
                n.ClickEvent.UserAgent == "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" &&
                n.ClickEvent.Referer == "http://unittest.com" &&
                n.ClickEvent.DeviceType == DeviceTypes.Desktop &&
                n.ClickEvent.Country == expectedCountry
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenShortCodeDoesNotExist()
    {
        // Arrange
        var nonExistentShortUrl = new ShortUrl("http://example.com", "nonexistent");

        _shortUrlRepository.GetByFilterAsync(
            Arg.Any<Expression<Func<ShortUrl, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns((ShortUrl?)null);

        var request = new GetByShortCodeRequest(
            "nonexistent",
            "UnitTestAgent",
            "http://unittest.com",
            "127.0.0.1"
        );

        // Act
        var result = await _query.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors!.First().ErrorMessage.Should().Be("Short code not found.");

        await _shortUrlRepository.Received(1).GetByFilterAsync(
            Arg.Any<Expression<Func<ShortUrl, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        await _publisher.DidNotReceive().Publish(
            Arg.Any<CreateClickEventNotification>(),
            Arg.Any<CancellationToken>()
        );
    }
}
