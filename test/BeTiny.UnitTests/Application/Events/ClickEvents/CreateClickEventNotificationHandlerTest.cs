using BeTiny.Application.Common.Interfaces.Repositories;
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
    private readonly ILogger<CreateClickEventNotificationHandler> _logger;
    private readonly CreateClickEventNotificationHandler _handler;

    public CreateClickEventNotificationHandlerTest()
    {
        _repository = Substitute.For<IRepository<ClickEvent, ClickEventId, Guid>>();
        _logger = Substitute.For<ILogger<CreateClickEventNotificationHandler>>();
        _handler = new (_repository, _logger);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ThenSavesClickEvent()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ClickEvent>().CustomInstantiator(
                f => new (
                    ShortUrlId.CreateUnique(),
                    f.Internet.IpAddress()
                        .MapToIPv4()
                        .ToString(),
                    f.Address.Country(),
                    f.Internet.UserAgent(),
                    f.Random.String2(10),
                    f.PickRandom<DeviceTypes>()
                )
            )
        );
    
        // Act
        await _handler.Handle(notification);
    
        // Assert
        await _repository.Received(1)
            .AddAsync(
                Arg.Is<ClickEvent>(ce => ce.Id == notification.ClickEvent.Id),
                Arg.Any<CancellationToken>()
            );

        await _repository.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUnsuccessfull_ThenThrowsException()
    {
        // Arrange
        var notification = new CreateClickEventNotification(
            new Faker<ClickEvent>().CustomInstantiator(
                f => new (
                    ShortUrlId.CreateUnique(),
                    f.Internet.IpAddress()
                        .MapToIPv4()
                        .ToString(),
                    f.Address.Country(),
                    f.Internet.UserAgent(),
                    f.Random.String2(10),
                    f.PickRandom<DeviceTypes>()
                )
            )
        );

        _repository.AddAsync(Arg.Any<ClickEvent>(),Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception());
    
        // Act
        var func = async () => await _handler.Handle(notification);
    
        // Assert
        await func.Should().ThrowAsync<Exception>();
        
        await _repository.Received(1)
            .AddAsync(
                Arg.Is<ClickEvent>(ce => ce.Id == notification.ClickEvent.Id),
                Arg.Any<CancellationToken>()
            );

        await _repository.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }
}
