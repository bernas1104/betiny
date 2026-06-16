using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Events.ClickEvents.Create;
using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;
using Bogus;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

namespace BeTiny.UnitTests.Application.Events.ClickEvents;

public class CreateClickEventNotificationHandlerTest
{
    private readonly IRepository<ClickEvent, ClickEventId, Guid> _repository;
    private readonly IIpResolver _ipResolver;
    private readonly IDeviceDetector _deviceDetector;
    private readonly ILogger<CreateClickEventNotificationHandler> _logger;
    private readonly CreateClickEventNotificationHandler _handler;
    private readonly Faker _faker = new();

    public CreateClickEventNotificationHandlerTest()
    {
        _repository = Substitute.For<IRepository<ClickEvent, ClickEventId, Guid>>();
        _ipResolver = Substitute.For<IIpResolver>();
        _deviceDetector = Substitute.For<IDeviceDetector>();
        _logger = Substitute.For<ILogger<CreateClickEventNotificationHandler>>();
        _handler = new (_repository, _ipResolver, _deviceDetector, _logger);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ThenSavesClickEvent()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ShortUrl>().CustomInstantiator(
                f => new (
                    "http://example.com",
                    f.PickRandom<AliasUrlType>()
                )
            ),
            _faker.Internet.UserAgent(),
            "referer",
            _faker.Internet.Ip()
        );
    
        // Act
        await _handler.Handle(notification);
    
        // Assert
        await _repository.Received(1)
            .AddAsync(
                Arg.Is<ClickEvent>(ce => ce.ShortUrlId == notification.ShortUrl.Id),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenUnsuccessfull_ThenThrowsException()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ShortUrl>().CustomInstantiator(
                f => new (
                    "http://example.com",
                    f.PickRandom<AliasUrlType>()
                )
            ).Generate(),
            _faker.Internet.UserAgent(),
            "referer",
            _faker.Internet.Ip()
        );

        _repository.AddAsync(Arg.Any<ClickEvent>(),Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception());
    
        // Act
        var func = async () => await _handler.Handle(notification);
    
        // Assert
        await func.Should().ThrowAsync<Exception>();
        
        await _repository.Received(1)
            .AddAsync(
                Arg.Is<ClickEvent>(ce => ce.ShortUrlId == notification.ShortUrl.Id),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task TryGetCountryByIpAsync_WhenIpResolutionFails_ThenReturnsUnknown()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ShortUrl>().CustomInstantiator(
                f => new (
                    "http://example.com",
                    f.PickRandom<AliasUrlType>()
                )
            ),
            _faker.Internet.UserAgent(),
            "referer",
            _faker.Internet.Ip()
        );

        _ipResolver.GetCountryByIpAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception());
    
        // Act
        await _handler.Handle(notification);
    
        // Assert
        _logger.ReceivedCalls()
            .Where(call => (LogLevel)call.GetArguments()[0]! == LogLevel.Warning
                && call.GetArguments()[3] is not null
                && call.GetArguments()[3]!.GetType() == typeof(Exception)
            )
            .Should()
            .ContainSingle();

        await _repository.Received(1)
            .AddAsync(
                Arg.Is<ClickEvent>(ce => ce.ShortUrlId == notification.ShortUrl.Id),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task TryGetCountryByIpAsync_WhenIpResolutionIsCancelled_ThenThrowsOperationCanceledException()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ShortUrl>().CustomInstantiator(
                f => new (
                    "http://example.com",
                    f.PickRandom<AliasUrlType>()
                )
            ),
            _faker.Internet.UserAgent(),
            "referer",
            _faker.Internet.Ip()
        );

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _ipResolver.GetCountryByIpAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());
    
        // Act
        var func = async () => await _handler.Handle(notification, cts.Token);
    
        // Assert
        await func.Should().ThrowAsync<OperationCanceledException>();
    }
}
